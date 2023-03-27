using Inertia.Logging;
using System;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Inertia.Network
{
    internal sealed class WebSocketNetworkProtocol : NetworkProtocol
    {
        private const string HttpProtocolKey = "HTTP/1.1";
        private const string WsHandshakeKey = "Sec-WebSocket-Key:";
        private const string WsStaticHexHashKey = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        public override int NetworkBufferLength => 8192;
        public override int ConnectionPerQueueInPool => 500;
        public override int ClientMessagePerQueueCapacity => 1000;
        public override int MaximumMessageCountPerSecond => 70;

        internal WebSocketNetworkProtocol()
        {
        }

        public override byte[] SerializeMessage(NetworkMessage message)
        {
            using (var writer = new BasicWriter())
            {
                writer
                    .SetUShort(message.MessageId)
                    .SetEmpty(sizeof(uint));

                var cPos = writer.GetPosition();

                message.Serialize(writer);
                writer
                    .SetPosition(cPos - sizeof(uint))
                    .SetUInt((uint)(writer.TotalLength - cPos));

                return writer.ToArray();
            }
        }
        public override bool ParseMessage(object receiver, BasicReader reader, MessageParsingOutput output)
        {
            reader.SetPosition(0);

            try
            {
                var isTcpConnection = receiver is WebSocketConnectionEntity;
                if (!isTcpConnection)
                {
                    throw new SocketException();
                }

                var connection = (WebSocketConnectionEntity)receiver;
                if (connection.State == WebSocketConnectionState.Connecting)
                {
                    var data = reader.GetBytes((int)reader.UnreadedLength);
                    var dataAsString = Encoding.UTF8.GetString(data);
                    var handshakeResult = GetHanshakeKeyResult(dataAsString);

                    if (string.IsNullOrWhiteSpace(handshakeResult))
                    {
                        throw new SocketException();
                    }

                    connection.SendHandshakeResponse(handshakeResult);
                    reader.RemoveReadedBytes();
                    return true;
                }

                while (reader.UnreadedLength > 0)
                {
                    if (TryParseWsMessage(reader, out var applicationData, out var opCode))
                    {
                        reader.RemoveReadedBytes();

                        if (opCode == WebSocketOpCode.ConnectionClose)
                        {
                            connection.SendSpecificOpCode(new byte[0], WebSocketOpCode.ConnectionClose);
                            connection.Disconnect(NetworkDisconnectReason.ConnectionLost);
                            break;
                        }
                        else if (opCode == WebSocketOpCode.Ping)
                        {
                            connection.SendSpecificOpCode(applicationData, WebSocketOpCode.Pong);
                            break;
                        }

                        using (var messageReader = new BasicReader(applicationData))
                        {
                            var msgId = messageReader.GetUShort();
                            var msgSize = messageReader.GetUInt();
                            var message = CreateMessage(msgId);
                            if (message == null)
                            {
                                messageReader
                                    .Skip((int)msgSize)
                                    .RemoveReadedBytes();

                                throw new UnknownMessageException(msgId);
                            }

                            message.Deserialize(messageReader);
                            output.AddMessage(message);
                        }
                    }
                    else break;                
                }

                return true;
            }
            catch
            {
                //Transform type to NetworkConnectionEntity for UDP support
                if (receiver is TcpConnectionEntity connection)
                {
                    connection.Disconnect(NetworkDisconnectReason.InvalidDataReceived);
                }
                else if (receiver is NetworkClientEntity client)
                {
                    client.Disconnect(NetworkDisconnectReason.InvalidDataReceived);
                }

                return false;
            }
        }

        internal byte[] WriteWsMessage(byte[] applicationData, WebSocketOpCode wsOpCode)
        {
            using (var writer = new BasicWriter())
            {
                var payloadSize = applicationData.Length < 126 ? applicationData.Length : (applicationData.Length <= ushort.MaxValue ? 126 : 127);
                byte opCode = (byte)wsOpCode;
                opCode |= 1 << 7;

                writer
                    .SetByte(opCode)
                    .SetByte((byte)payloadSize);

                if (payloadSize == 126)
                {
                    var bytes = BitConverter.GetBytes((ushort)applicationData.Length);
                    Array.Reverse(bytes);

                    writer.SetBytesWithoutHeader(bytes);
                }
                else if (payloadSize == 127)
                {
                    var bytes = BitConverter.GetBytes((uint)applicationData.Length);
                    Array.Reverse(bytes);

                    writer.SetBytesWithoutHeader(bytes);
                }

                return writer
                    .SetBytesWithoutHeader(applicationData)
                    .ToArray();
            }
        }

        private string GetHanshakeKeyResult(string data)
        {
            if (data.Contains(HttpProtocolKey))
            {
                var handshakeKeyLine = data.Split("\r\n")
                    .FirstOrDefault((line) => line.Trim().StartsWith(WsHandshakeKey));

                if (!string.IsNullOrWhiteSpace(handshakeKeyLine))
                {
                    var sKey = $"{handshakeKeyLine.Split(':')[1].Trim()}{WsStaticHexHashKey}";
                    using (var sha1 = new SHA1Managed())
                    {
                        var hashData = sha1.ComputeHash(Encoding.ASCII.GetBytes(sKey));
                        return Convert.ToBase64String(hashData);
                    }
                }
            }

            return null;
        }
        private bool TryParseWsMessage(BasicReader reader, out byte[] applicationData, out WebSocketOpCode opCode)
        {
            //'Fin' not supported (always considered as true)
            //ExtensionData not supported

            var fByte = reader.GetByte();
            var fin = fByte.GetBit(0, EndiannessType.BigEndian);
            var payloadByte = reader.GetByte();
            var masked = payloadByte.GetBit(0, EndiannessType.BigEndian);

            opCode = (WebSocketOpCode)fByte.SetBit(0, false, EndiannessType.BigEndian);
            payloadByte.SetBit(0, false, EndiannessType.BigEndian);

            int appDataSize = payloadByte;
            if (payloadByte == 126)
            {
                var bytes = reader.GetBytes(2);
                Array.Reverse(bytes);

                appDataSize = BitConverter.ToUInt16(bytes);
            }
            else if (payloadByte == 127)
            {
                var bytes = reader.GetBytes(4);
                Array.Reverse(bytes);

                appDataSize = (int)BitConverter.ToUInt64(bytes);
            }

            if (reader.UnreadedLength < appDataSize || masked && reader.UnreadedLength < appDataSize + 4)
            {
                applicationData = new byte[0];
                return false;
            }

            if (masked)
            {
                var maskingKey = reader.GetBytes(4);
                var data = reader.GetBytes(appDataSize);

                for (var i = 0; i < data.Length; i++)
                {
                    data[i] = (byte)(data[i] ^ maskingKey[i % 4]);
                }

                applicationData = data;
            }
            else
            {
                applicationData = reader.GetBytes(appDataSize);
            }

            return true;
        }
    }
}
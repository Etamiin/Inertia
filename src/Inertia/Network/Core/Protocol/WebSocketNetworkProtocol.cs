using Inertia.Logging;
using System;
using System.Linq;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;

namespace Inertia.Network
{
    internal sealed class WebSocketNetworkProtocol : NetworkProtocol
    {
        private const string HttpProtocolKey = "HTTP/1.1";
        private const string WsHandshakeKey = "Sec-WebSocket-Key:";
        private const string WsHexHashKeyConst = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
        private const byte PayloadMidSize = 126;
        private const byte PayloadFullSize = 127;

        public override int NetworkBufferLength => 4096;
        public override int ConnectionPerQueueInPool => 750;

        internal WebSocketNetworkProtocol()
        {
        }

        public override byte[] SerializeMessage(NetworkMessage message)
        {
            using (var writer = new BasicWriter())
            {
                writer.SetUShort(message.MessageId);
                message.Serialize(writer);

                return writer.ToArray();
            }
        }
        public override bool ParseMessage(NetworkEntity receiver, BasicReader reader, MessageParsingOutput output)
        {
            reader.SetPosition(0);

            try
            {
                if (!(receiver is WebSocketConnectionEntity connection))
                {
                    throw new WebSocketException($"The client is not a valid {nameof(WebSocketConnectionEntity)}.");
                }

                if (connection.ConnectionState == WebSocketConnectionState.Connecting)
                {
                    TryProcessHandshakeMessage(reader, connection);
                    return true;
                }

                while (reader.UnreadedLength > 0)
                {
                    if (!TryParseMessage(reader, out var applicationData, out var opCode)) break;

                    reader.RemoveReadedBytes();
                    if (ProcessOpCodeMessages(connection, opCode, ref applicationData)) break;

                    using (var messageReader = new BasicReader(applicationData))
                    {
                        while (messageReader.UnreadedLength > 0)
                        {
                            var msgId = messageReader.GetUShort();
                            var message = CreateMessageById(msgId);
                            if (message == null)
                            {
                                throw new UnknownMessageException(msgId);
                            }

                            message.Deserialize(messageReader);
                            output.AddMessage(message);

                            if (messageReader.UnreadedLength > 0) messageReader.RemoveReadedBytes();
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                BasicLogger.Default.Error("Parsing network message failed: " + ex);

                if (receiver is NetworkConnectionEntity connection)
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

        internal byte[] WriteMessage(byte[] applicationData, WebSocketOpCode wsOpCode)
        {
            using (var writer = new BasicWriter())
            {
                var payloadSize = applicationData.Length < PayloadMidSize ? applicationData.Length : (applicationData.Length <= ushort.MaxValue ? PayloadMidSize : PayloadFullSize);
                byte opCode = (byte)wsOpCode;
                opCode.SetBitRef(0, true, EndiannessType.BigEndian);

                writer
                    .SetByte(opCode)
                    .SetByte((byte)payloadSize);

                if (payloadSize == PayloadMidSize)
                {
                    var bytes = BitConverter.GetBytes((ushort)applicationData.Length);
                    Array.Reverse(bytes);

                    writer.SetBytesWithoutHeader(bytes);
                }
                else if (payloadSize == PayloadFullSize)
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

        private void TryProcessHandshakeMessage(BasicReader reader, WebSocketConnectionEntity connection)
        {
            var data = reader.GetBytes((int)reader.UnreadedLength);
            var httpRequest = Encoding.UTF8.GetString(data);
            var handshakeResult = GetHanshakeKeyResult(httpRequest);

            if (string.IsNullOrWhiteSpace(handshakeResult))
            {
                throw new WebSocketException(WebSocketError.HeaderError);
            }

            reader.RemoveReadedBytes();
            connection.SendHandshakeResponse(handshakeResult);
        }
        private string GetHanshakeKeyResult(string request)
        {
            if (request.Contains(HttpProtocolKey))
            {
                var handshakeKeyLine = request.Split("\r\n")
                    .FirstOrDefault((line) => line.Trim().StartsWith(WsHandshakeKey));

                if (!string.IsNullOrWhiteSpace(handshakeKeyLine))
                {
                    var sKey = $"{handshakeKeyLine.Split(':')[1].Trim()}{WsHexHashKeyConst}";
                    using (var sha1 = new SHA1Managed())
                    {
                        var hashData = sha1.ComputeHash(Encoding.ASCII.GetBytes(sKey));
                        return Convert.ToBase64String(hashData);
                    }
                }
            }

            return null;
        }
        private bool ProcessOpCodeMessages(WebSocketConnectionEntity connection, WebSocketOpCode opCode, ref byte[] applicationData)
        {
            if (opCode == WebSocketOpCode.ConnectionClose)
            {
                connection.SendSpecificOpCode(applicationData, WebSocketOpCode.ConnectionClose);
                connection.Disconnect(NetworkDisconnectReason.ConnectionLost);

                return true;
            }
            else if (opCode == WebSocketOpCode.Ping)
            {
                connection.SendSpecificOpCode(applicationData, WebSocketOpCode.Pong);

                return true;
            }

            return false;
        }
        private bool TryParseMessage(BasicReader reader, out byte[] applicationData, out WebSocketOpCode opCode)
        {
            //Field 'fin' not supported (always considered as true)
            //ExtensionData not supported

            var fByte = reader.GetByte();
            var fin = fByte.GetBit(0, EndiannessType.BigEndian);
            var payloadByte = reader.GetByte();
            var masked = payloadByte.GetBit(0, EndiannessType.BigEndian);

            opCode = (WebSocketOpCode)fByte.SetBit(0, false, EndiannessType.BigEndian);
            payloadByte.SetBitRef(0, false, EndiannessType.BigEndian);

            int appDataSize = payloadByte;
            if (payloadByte == PayloadMidSize)
            {
                var bytes = reader.GetBytes(2);
                Array.Reverse(bytes);

                appDataSize = BitConverter.ToUInt16(bytes);
            }
            else if (payloadByte == PayloadFullSize)
            {
                var bytes = reader.GetBytes(4);
                Array.Reverse(bytes);

                appDataSize = (int)BitConverter.ToUInt64(bytes);
            }

            if (reader.UnreadedLength < appDataSize || masked && reader.UnreadedLength < appDataSize + 4) //+4 for masking key
            {
                applicationData = new byte[0];
                return false;
            }

            if (masked)
            {
                var maskingKey = reader.GetBytes(4);
                applicationData = reader.GetBytes(appDataSize);

                for (var i = 0; i < applicationData.Length; i++)
                {
                    applicationData[i] = (byte)(applicationData[i] ^ maskingKey[i % 4]);
                }
            }
            else
            {
                applicationData = reader.GetBytes(appDataSize);
            }

            return true;
        }
    }
}
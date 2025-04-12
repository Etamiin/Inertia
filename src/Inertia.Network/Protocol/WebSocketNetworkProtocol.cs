using Inertia.IO;
using System;
using System.Linq;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;

namespace Inertia.Network
{
    public abstract class WebSocketNetworkProtocol : NetworkProtocol
    {
        private const string HttpProtocolKey = "HTTP/1.1";
        private const string HandshakeKey = "Sec-WebSocket-Key:";
        private const string HexHashKeyConstant = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
        private const byte PayloadMidSize = 126;
        private const byte PayloadFullSize = 127;

        private protected WebSocketNetworkProtocol()
        {
        }

        public override byte[] SerializeMessage(NetworkMessage message)
        {
            using (var writer = new DataWriter())
            {
                writer
                    .Write(message.MessageId)
                    .Write(message);

                return writer.ToArray();
            }
        }
        public override bool TryDeserializeMessage(NetworkEntity receiver, DataReader reader, MessageParsingOutput output)
        {
            reader.Position = 0;

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

                long lastFullyReadedPos = 0;

                while (reader.UnreadedLength > 0)
                {
                    if (!TryParseMessage(reader, out var applicationData, out var opCode)) break;

                    lastFullyReadedPos = reader.Position;

                    if (ProcessOpCodeMessages(connection, opCode, ref applicationData)) break;

                    using (var messageReader = new DataReader(applicationData))
                    {
                        while (messageReader.UnreadedLength > 0)
                        {
                            var msgId = messageReader.ReadUShort();

                            if (!NetworkManager.TryGetMessageType(msgId, out var messageType))
                            {
                                throw new UnknownMessageException(msgId);
                            }

                            var message = (NetworkMessage)messageReader.ReadValue(messageType);
                            output.AddMessage(message);

                            if (messageReader.UnreadedLength > 0) messageReader.RemoveReadedBytes();
                        }
                    }
                }

                if (lastFullyReadedPos != 0)
                {
                    reader.Position = lastFullyReadedPos;
                    reader.RemoveReadedBytes();
                }

                return true;
            }
            catch (Exception ex)
            {
                receiver.Disconnect(NetworkStopReason.InvalidDataReceived);
                return false;
            }
        }
        public virtual byte[] WriteWebSocketMessage(byte[] data, WebSocketOpCode opCode)
        {
            using (var writer = new DataWriter())
            {
                var payloadSize = data.Length < PayloadMidSize ? data.Length : (data.Length <= ushort.MaxValue ? PayloadMidSize : PayloadFullSize);
                byte _opCode = (byte)opCode;
                _opCode.SetBitByRef(0, true, EndiannessType.BigEndian);

                writer
                    .Write(_opCode)
                    .Write((byte)payloadSize);

                if (payloadSize == PayloadMidSize)
                {
                    var length = (ushort)data.Length;

                    writer.Write(new[] { (byte)(length >> 8), (byte)(length & 0xFF) });
                }
                else if (payloadSize == PayloadFullSize)
                {
                    var length = (uint)data.Length;

                    writer.Write(new[] {
                        (byte)(length >> 24),
                        (byte)((length >> 16) & 0xFF),
                        (byte)((length >> 8) & 0xFF),
                        (byte)(length & 0xFF)
                    });
                }

                return writer
                    .Write(data)
                    .ToArray();
            }
        }

        private void TryProcessHandshakeMessage(DataReader reader, WebSocketConnectionEntity connection)
        {
            var data = reader.ReadBytes((int)reader.UnreadedLength);
            var httpRequest = Encoding.UTF8.GetString(data);
            var handshakeResult = GetHanshakeKeyResult();

            if (string.IsNullOrWhiteSpace(handshakeResult))
            {
                throw new WebSocketException(WebSocketError.HeaderError);
            }

            reader.RemoveReadedBytes();
            connection.SendHandshakeResponse(handshakeResult);

            string? GetHanshakeKeyResult()
            {
                if (httpRequest.Contains(HttpProtocolKey))
                {
                    var handshakeKeyLine = httpRequest.Split("\r\n")
                        .FirstOrDefault((line) => line.Trim().StartsWith(HandshakeKey));

                    if (!string.IsNullOrWhiteSpace(handshakeKeyLine))
                    {
                        var sKey = $"{handshakeKeyLine.Split(':')[1].Trim()}{HexHashKeyConstant}";
                        using (var sha1 = new SHA1Managed())
                        {
                            var hashData = sha1.ComputeHash(Encoding.ASCII.GetBytes(sKey));
                            return Convert.ToBase64String(hashData);
                        }
                    }
                }

                return null;
            }
        }
        private bool ProcessOpCodeMessages(WebSocketConnectionEntity connection, WebSocketOpCode opCode, ref byte[] data)
        {
            if (opCode == WebSocketOpCode.ConnectionClose)
            {
                connection.SendSpecificOpCode(data, WebSocketOpCode.ConnectionClose);
                connection.Disconnect(NetworkStopReason.ConnectionLost);

                return true;
            }
            else if (opCode == WebSocketOpCode.Ping)
            {
                connection.SendSpecificOpCode(data, WebSocketOpCode.Pong);

                return true;
            }

            return false;
        }
        private bool TryParseMessage(DataReader reader, out byte[] data, out WebSocketOpCode opCode)
        {
            //Field 'fin' not supported (always considered as true)
            //ExtensionData not supported

            var fByte = reader.ReadByte();
            //_ = fByte.GetBit(0, EndiannessType.BigEndian); // fin
            var payloadByte = reader.ReadByte();
            var masked = payloadByte.GetBit(0, EndiannessType.BigEndian);

            opCode = (WebSocketOpCode)fByte.SetBit(0, false, EndiannessType.BigEndian);
            payloadByte.SetBitByRef(0, false, EndiannessType.BigEndian);

            int appDataSize = payloadByte;
            if (payloadByte == PayloadMidSize)
            {
                var bytes = reader.ReadBytes(2);
                Array.Reverse(bytes);

                appDataSize = BitConverter.ToUInt16(bytes);
            }
            else if (payloadByte == PayloadFullSize)
            {
                var bytes = reader.ReadBytes(4);
                Array.Reverse(bytes);

                appDataSize = (int)BitConverter.ToUInt64(bytes);
            }

            if (reader.UnreadedLength < appDataSize || masked && reader.UnreadedLength < appDataSize + 4) //+4 for masking key
            {
                data = new byte[0];
                return false;
            }

            if (masked)
            {
                var maskingKey = reader.ReadBytes(4);
                data = reader.ReadBytes(appDataSize);

                for (var i = 0; i < data.Length; i++)
                {
                    data[i] = (byte)(data[i] ^ maskingKey[i % 4]);
                }
            }
            else
            {
                data = reader.ReadBytes(appDataSize);
            }

            return true;
        }
    }
}
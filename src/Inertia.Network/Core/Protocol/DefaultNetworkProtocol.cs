using Inertia.Logging;
using System;

namespace Inertia.Network
{
    internal sealed class DefaultNetworkProtocol : NetworkProtocol
    {
        public override int NetworkBufferLength => 4096;
        public override int ConnectionPerMessageQueue => 500;

        private ILogger _logger;

        internal DefaultNetworkProtocol(ILogger logger)
        {
            _logger = logger ?? NullLogger.Instance;
        }

        public override byte[] SerializeMessage(NetworkMessage message)
        {
            using (var writer = new DataWriter())
            {
                writer
                    .Write(message.MessageId)
                    .SetEmpty(sizeof(uint));

                var cPos = writer.GetPosition();

                message.Serialize(writer);
                writer
                    .SetPosition(cPos - sizeof(uint))
                    .Write((uint)(writer.TotalLength - cPos));

                return writer.ToArray();
            }
        }
        public override bool TryParseMessage(NetworkEntity receiver, DataReader reader, MessageParsingOutput output)
        {
            reader.SetPosition(0);

            try
            {
                long lastFullyReadedPos = 0;

                while (reader.UnreadedLength > 0)
                {
                    var msgId = reader.ReadUShort();
                    var msgSize = reader.ReadUInt();

                    if (reader.UnreadedLength < msgSize) break;

                    var message = CreateMessageById(msgId);
                    if (message is null)
                    {
                        reader
                            .Skip((int)msgSize)
                            .RemoveReadedBytes();

                        throw new UnknownMessageException(msgId);
                    }

                    message.Deserialize(reader.ReadByte(), reader);
                    output.AddMessage(message);

                    lastFullyReadedPos = reader.GetPosition();
                }

                if (lastFullyReadedPos != 0)
                {
                    reader
                        .SetPosition(lastFullyReadedPos)
                        .RemoveReadedBytes();
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, GetType(), nameof(TryParseMessage));

                receiver.Disconnect(NetworkDisconnectReason.InvalidDataReceived);
                return false;
            }            
        }
    }
}
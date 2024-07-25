using Inertia.Logging;
using System;

namespace Inertia.Network
{
    internal sealed class DefaultNetworkProtocol : NetworkProtocol
    {
        public override int NetworkBufferLength => 4096;
        public override int ConnectionPerMessageQueue => 500;

        internal DefaultNetworkProtocol()
        {
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
                while (reader.UnreadedLength > 0)
                {
                    var msgId = reader.ReadUShort();
                    var msgSize = reader.ReadUInt();

                    if (reader.UnreadedLength < msgSize) break;

                    var message = CreateMessageById(msgId);
                    if (message == null)
                    {
                        reader
                            .Skip((int)msgSize)
                            .RemoveReadedBytes();

                        throw new UnknownMessageException(msgId);
                    }

                    message.Deserialize(reader);
                    output.AddMessage(message);
                    reader.RemoveReadedBytes();
                }

                return true;
            }
            catch (Exception ex)
            {
                LoggingProvider.Logger.Error(ex, GetType(), nameof(TryParseMessage));

                receiver.Disconnect(NetworkDisconnectReason.InvalidDataReceived);
                return false;
            }            
        }
    }
}
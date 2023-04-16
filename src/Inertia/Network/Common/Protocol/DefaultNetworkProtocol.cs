using Inertia.Logging;
using System;

namespace Inertia.Network
{
    internal sealed class DefaultNetworkProtocol : NetworkProtocol
    {
        public override int NetworkBufferLength => 4096;
        public override int ConnectionPerQueueInPool => 750;

        internal DefaultNetworkProtocol()
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
        public override bool ParseMessage(NetworkEntity receiver, BasicReader reader, MessageParsingOutput output)
        {
            reader.SetPosition(0);

            try
            {
                while (reader.UnreadedLength > 0)
                {
                    var msgId = reader.GetUShort();
                    var msgSize = reader.GetUInt();

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
            catch
            {
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
    }
}
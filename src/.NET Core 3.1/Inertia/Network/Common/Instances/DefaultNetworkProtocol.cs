using System;

namespace Inertia.Network
{
    /// <summary>
    /// Represents the default network protocol used by network entities.
    /// </summary>
    public sealed class DefaultNetworkProtocol : NetworkProtocol
    {
        public override bool PooledExecution => false;
        public override int NetworkBufferLength => 8192;

        internal DefaultNetworkProtocol()
        {
        }

        /// <summary>
        /// Occurs when a <see cref="NetworkMessage"/> is requested to be written before being sent.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public override byte[] OnSerializeMessage(NetworkMessage message)
        {
            using (var writer = new BasicWriter())
            {
                writer
                    .SetUInt(message.MessageId)
                    .SetEmpty(sizeof(long));

                var cPos = writer.GetPosition();

                message.Serialize(writer);

                writer
                    .SetPosition(cPos - sizeof(long))
                    .SetLong(writer.TotalLength - cPos);

                return writer.ToArrayAndDispose();
            }
        }

        public override void OnParseMessage(object receiver, BasicReader reader, MessageParsingOutput output)
        {
            reader.SetPosition(0);

            while (reader.UnreadedLength > 0)
            {
                var msgId = reader.GetUInt();
                var msgSize = reader.GetLong();

                if (reader.UnreadedLength < msgSize) break;

                try
                {
                    var message = CreateMessage(msgId);
                    if (message == null)
                    {
                        throw new UnknownMessageException(msgId);
                    }

                    message.Deserialize(reader);
                    output.AddOutput(message);
                }
                catch (Exception ex)
                {
                    throw new DefaultProtocolFailedParsingMessageException(ex.Message);
                }

                reader.RemoveReadedBytes();
            }
        }
    }
}

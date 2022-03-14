using System;

namespace Inertia.Network
{
    /// <summary>
    /// Represents the default network protocol used by network entities.
    /// </summary>
    public sealed class DefaultNetworkProtocol : NetworkProtocol
    {
        public override int NetworkBufferLength => 4096;
        public override int ConnectionPerQueueInPool => 750;
        public override int ClientMessagePerQueueCapacity => 1000;
        public override int AuthorizedDataCountPerSecond => 55;

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
                    .SetUShort(message.MessageId)
                    .SetEmpty(sizeof(uint));

                var cPos = writer.GetPosition();

                message.Serialize(writer);
                writer
                    .SetPosition(cPos - sizeof(uint))
                    .SetUInt((uint)(writer.TotalLength - cPos));

                return writer.ToArrayAndDispose();
            }
        }
        /// <summary>
        /// Occurs when data are received and have to be parsed
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="reader"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        /// <exception cref="DefaultProtocolFailedParsingMessageException"></exception>
        public override void OnParseMessage(object receiver, BasicReader reader, MessageParsingOutput output)
        {
            reader.SetPosition(0);
            while (reader.UnreadedLength > 0)
            {
                var msgId = reader.GetUShort();
                var msgSize = reader.GetUInt();

                if (reader.UnreadedLength < msgSize) break;

                try
                {
                    var message = CreateMessage(msgId);
                    if (message == null)
                    {
                        reader.Skip((int)msgSize);
                        throw new UnknownMessageException(msgId);
                    }

                    message.Deserialize(reader);
                    output.AddMessage(message);
                }
                catch (Exception ex)
                {
                    OnParsingError(receiver, ex);
                }

                reader.RemoveReadedBytes();
            }
        }
        public override void OnParsingError(object receiver, Exception ex)
        {
            if (ex is UnknownMessageException)
            {
                if (receiver is TcpConnectionEntity connection)
                {
                    connection.Disconnect(NetworkDisconnectReason.SendingBadInformation);
                }
            }

            Log.Error($"NetworkProtocol Parsing Error: {ex}");
        }
    }
}
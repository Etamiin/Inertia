namespace Inertia.Network
{
    /// <summary>
    /// Represents the default network protocol used by network entities.
    /// </summary>
    public sealed class DefaultNetworkProtocol : NetworkProtocol
    {
        public override int NetworkBufferLength => 4096;
        public override int ConnectionPerQueueInPool => 500;
        public override int ClientMessagePerQueueCapacity => 1000;
        public override int MaximumMessageCountPerSecond => 55;

        internal DefaultNetworkProtocol()
        {
        }

        /// <summary>
        /// Occurs when a <see cref="NetworkMessage"/> is requested to be written before being sent.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
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
        public override bool ParseMessage(object receiver, BasicReader reader, MessageParsingOutput output)
        {
            reader.SetPosition(0);

            try 
            {
                while (reader.UnreadedLength > 0)
                {
                    var msgId = reader.GetUShort();
                    var msgSize = reader.GetUInt();

                    if (reader.UnreadedLength < msgSize) break;

                    var message = CreateMessage(msgId);
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
    }
}
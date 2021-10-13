using System;

namespace Inertia.Network
{
    /// <summary>
    /// Represents the default network protocol used by network entities.
    /// </summary>
    public sealed class DefaultNetworkProtocol : NetworkProtocol
    {
        /// <summary>
        /// 
        /// </summary>
        public static DefaultNetworkProtocol Instance { get; private set; }

        internal static void Initialize()
        {
            if (Instance == null)
            {
                new DefaultNetworkProtocol();
            }
        }

        /// <summary>
        /// Specify the version of the network protocol that can be used.
        /// </summary>
        public override ushort ProtocolVersion => 1;

        private readonly AutoQueueExecutor _queue;

        internal DefaultNetworkProtocol()
        {
            Instance = this;
            _queue = new AutoQueueExecutor();

            if (GetProtocol() == null)
            {
                SetProtocol(this);
            }
        }

        /// <summary>
        /// Occurs when a <see cref="NetworkMessage"/> is requested to be written before being sent.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public override byte[] OnParseMessage(NetworkMessage message)
        {
            using (var messageWriter = new BasicWriter())
            {
                messageWriter.SetUInt(message.MessageId);

                var cPos = messageWriter.Position;

                messageWriter.SetLong(messageWriter.Position - cPos);
                message.OnSerialize(messageWriter);

                return messageWriter.ToArrayAndDispose();
            }
        }

        /// <summary>
        /// Occurs when data is received from a <see cref="TcpClientEntity"/>.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="reader"></param>
        public override void OnReceiveData(TcpClientEntity client, BasicReader reader)
        {
            DefaultParseData(reader, (message) => GetCaller(message)?.TryCall(message, client));
        }
        /// <summary>
        /// Occurs when data is received from a <see cref="UdpClientEntity"/>.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="reader"></param>
        public override void OnReceiveData(UdpClientEntity client, BasicReader reader)
        {
            DefaultParseData(reader, (message) => GetCaller(message)?.TryCall(message, client));
        }
        /// <summary>
        /// Occurs when data is received from a <see cref="TcpConnectionEntity"/>.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="reader"></param>
        public override void OnReceiveData(TcpConnectionEntity connection, BasicReader reader)
        {
            DefaultParseData(reader, (message) => GetCaller(message)?.TryCall(message, connection));
        }
        /// <summary>
        /// Occurs when data is received from a <see cref="UdpConnectionEntity"/>.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="reader"></param>
        public override void OnReceiveData(UdpConnectionEntity connection, BasicReader reader)
        {
            DefaultParseData(reader, (message) => GetCaller(message)?.TryCall(message, connection));
        }

        private void DefaultParseData(BasicReader reader, BasicAction<NetworkMessage> onMessageParsed)
        {
            while (reader.UnreadedLength > 0)
            {
                reader.Position = 0;

                var msgId = reader.GetUInt();
                var msgSize = reader.GetLong();

                if (reader.UnreadedLength < msgSize)
                {
                    break;
                }

                try
                {
                    var message = CreateMessage(msgId);
                    if (message == null)
                    {
                        throw new UnknownMessageException(msgId);
                    }

                    message.OnDeserialize(reader);

                    if (MultiThreadedExecution)
                    {
                        onMessageParsed(message);
                    }
                    else
                    {
                        _queue.Enqueue(() => onMessageParsed(message));
                    }
                }
                catch (Exception ex)
                {
                    throw new DefaultProtocolFailedParseNetworkMessageException(ex.Message);
                }

                reader.RemoveReadedBytes();
            }
        }
    }
}

﻿namespace Inertia.Network
{
    public abstract class NetworkProtocol
    {
        public abstract int NetworkBufferLength { get; }
        public abstract int ConnectionPerQueueInPool { get; }

        protected NetworkProtocol()
        {
        }

        public abstract byte[] SerializeMessage(NetworkMessage message);
        public abstract bool ParseMessage(NetworkEntity receiver, BasicReader reader, MessageParsingOutput output);

        protected NetworkMessage? CreateMessageById(ushort messageId)
        {
            if (ReflectionProvider.TryGetMessageType(messageId, out var messageType))
            {
                return NetworkProtocolManager.CreateMessage(messageType);
            }

            return null;
        }
    }
}
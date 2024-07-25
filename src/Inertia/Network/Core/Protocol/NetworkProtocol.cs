namespace Inertia.Network
{
    public abstract class NetworkProtocol
    {
        public abstract int NetworkBufferLength { get; }
        public abstract int ConnectionPerMessageQueue { get; }

        protected NetworkProtocol()
        {
        }

        public abstract byte[] SerializeMessage(NetworkMessage message);
        public abstract bool TryParseMessage(NetworkEntity receiver, DataReader reader, MessageParsingOutput output);

        protected NetworkMessage? CreateMessageById(ushort messageId)
        {
            if (ReflectionProvider.TryCreateNetworkMessage(messageId, out var message)) return message;

            return null;
        }
    }
}
using Inertia.IO;

namespace Inertia.Network
{
    public abstract class NetworkProtocol
    {
        protected NetworkProtocol()
        {
        }

        public abstract int NetworkBufferLength { get; }
        public abstract int MaxReceivedMessagePerSecondPerClient { get; }

        public abstract byte[] SerializeMessage(NetworkMessage message);
        public abstract bool TryDeserializeMessage(NetworkEntity receiver, DataReader reader, MessageParsingOutput output);
    }
}
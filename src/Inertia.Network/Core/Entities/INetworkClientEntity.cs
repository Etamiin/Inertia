namespace Inertia.Network
{ 
    [IndirectNetworkEntity]
    public interface INetworkClientEntity
    {
        public object? State { get; set; }

        void Send(byte[] dataToSend);
        void Send(NetworkMessage message);
    }
}

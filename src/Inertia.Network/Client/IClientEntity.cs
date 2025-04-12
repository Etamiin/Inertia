namespace Inertia.Network
{ 
    [IndirectNetworkEntity]
    public interface IClientEntity
    {
        public object? State { get; set; }

        void Send(byte[] data);
        void Send(NetworkMessage message);
    }
}

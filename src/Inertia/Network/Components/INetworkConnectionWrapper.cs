using System.Security.Cryptography;

namespace Inertia.Network
{
    [IndirectNetworkEntity]
    public interface INetworkConnectionWrapper
    {
        public object? State { get; set; }

        public T GetStateAs<T>();
        public void Send(NetworkMessage message);
    }
}
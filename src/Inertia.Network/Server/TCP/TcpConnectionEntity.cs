using System.Net.Sockets;

namespace Inertia.Network
{
    public sealed class TcpConnectionEntity : TcpConnectionEntityBase
    {
        internal TcpConnectionEntity(Socket socket, uint id) : base(socket, id, NetworkManager.TcpProtocol)
        {
        }
    }
}
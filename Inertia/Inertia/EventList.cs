using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Inertia
{
    public delegate void DebugHandler(string log);
    public delegate void LoopContextHandler();
    public delegate void ActionHandler();
    public delegate void ContainerHandler();
    public delegate void ActionHandler<T>(T value);

    public delegate void ClientConnectedHandler();
    public delegate void ClientDisconnectedHandler(DisconnectReason reason);

    public delegate void ServerStartedHandler();
    public delegate void ServerStoppedHandler();
    public delegate void ServerTcpUserStateChangeHandler(TransmissionControl.User user);
    public delegate void ServerTcpUserDisconnectedHandler(TransmissionControl.User user, DisconnectReason reason);
    public delegate void ServerUdpUserStateChangeHandler(DatagramUser user);

    public delegate void OnStorageUpdateStateChangeHandler(StorageUpdate update);
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inertia;
using Inertia.Network;

#region Network Events

/// <summary>
/// Network disconnecting handler
/// </summary>
/// <param name="reason"></param>
public delegate void NetworkDisconnectHandler(NetworkDisconnectReason reason);
/// <summary>
/// Network transmission control protocol created handler
/// </summary>
/// <param name="connection"></param>
public delegate void NetworkTcpClientConnectionCreatedHandler(NetTcpConnection connection);
/// <summary>
/// Network transmission control protocol disconnected handler
/// </summary>
/// <param name="connection"></param>
/// <param name="reason"></param>
public delegate void NetworkTcpClientConnectionDisconnectedHandler(NetTcpConnection connection, NetworkDisconnectReason reason);
/// <summary>
/// Network group sender handler
/// </summary>
/// <param name="packet"></param>
public delegate void NetworkGroupSenderHandler(NetworkMessage packet);
/// <summary>
/// Network user datagram added handler
/// </summary>
/// <param name="connection"></param>
public delegate void NetworkUdpConnectionAddedHandler(NetUdpConnection connection);

#endregion
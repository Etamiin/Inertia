using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inertia.Network;

/// <summary>
/// 
/// </summary>
public delegate void BasicAction();
/// <summary>
///
/// </summary>
public delegate void BasicAction<T>(T value);
/// <summary>
/// 
/// </summary>
public delegate void BasicAction<T1, T2>(T1 value1, T2 value2);
/// <summary>
/// 
/// </summary>
public delegate void BasicAction<T1, T2, T3>(T1 value1, T2 value2, T3 value3);
/// <summary>
/// 
/// </summary>
public delegate void BasicAction<T1, T2, T3, T4>(T1 value1, T2 value2, T3 value3, T4 value4);
/// <summary>
/// 
/// </summary>
public delegate void BasicAction<T1, T2, T3, T4, T5>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5);
/// <summary>
/// 
/// </summary>
public delegate R BasicReturnAction<R>();
/// <summary>
/// 
/// </summary>
public delegate R BasicReturnAction<T1, R>(T1 value1);
/// <summary>
///
/// </summary>
public delegate R BasicReturnAction<T1, T2, R>(T1 value1, T2 value2);
/// <summary>
///
/// </summary>
public delegate R BasicReturnAction<T1, T2, T3, R>(T1 value1, T2 value2, T3 value3);
/// <summary>
///
/// </summary>
public delegate R BasicReturnAction<T1, T2, T3, T4, R>(T1 value1, T2 value2, T3 value3, T4 value4);
/// <summary>
///
/// </summary>
public delegate R BasicReturnAction<T1, T2, T3, T4, T5, R>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5);

//NETWORK

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
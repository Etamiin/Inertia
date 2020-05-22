using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inertia;
using Inertia.Network;
using Inertia.Storage;

#region General Events

/// <summary>
/// Execute code
/// </summary>
public delegate void SimpleAction();
/// <summary>
/// Execute code with <typeparamref name="T"/> parameter
/// </summary>
public delegate void SimpleAction<T>(T value);
/// <summary>
/// Execute code with multiples types parameters
/// </summary>
public delegate void SimpleAction<T1, T2>(T1 value1, T2 value2);
/// <summary>
/// Execute code with multiples types parameters
/// </summary>
public delegate void SimpleAction<T1, T2, T3>(T1 value1, T2 value2, T3 value3);
/// <summary>
/// Execute code with multiples types parameters
/// </summary>
public delegate void SimpleAction<T1, T2, T3, T4>(T1 value1, T2 value2, T3 value3, T4 value4);
/// <summary>
/// Execute code with multiples types parameters
/// </summary>
public delegate void SimpleAction<T1, T2, T3, T4, T5>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5);
/// <summary>
/// Execute code with a <typeparamref name="R"/> returned object
/// </summary>
public delegate R SimpleReturnAction<R>();
/// <summary>
/// Execute code with a <typeparamref name="R"/> returned object and one <typeparamref name="T1"/> parameter
/// </summary>
public delegate R SimpleReturnAction<T1, R>(T1 value1);
/// <summary>
/// Execute code with a <typeparamref name="R"/> returned object and multiples types parameters
/// </summary>
public delegate R SimpleReturnAction<T1, T2, R>(T1 value1, T2 value2);
/// <summary>
/// Execute code with a <typeparamref name="R"/> returned object and multiples types parameters
/// </summary>
public delegate R SimpleReturnAction<T1, T2, T3, R>(T1 value1, T2 value2, T3 value3);
/// <summary>
/// Execute code with a <typeparamref name="R"/> returned object and multiples types parameters
/// </summary>
public delegate R SimpleReturnAction<T1, T2, T3, T4, R>(T1 value1, T2 value2, T3 value3, T4 value4);
/// <summary>
/// Execute code with a <typeparamref name="R"/> returned object and multiples types parameters
/// </summary>
public delegate R SimpleReturnAction<T1, T2, T3, T4, T5, R>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5);

#endregion

#region Storage Events

/// <summary>
/// Storage event failing handler
/// </summary>
/// <param name="ex"></param>
public delegate void StorageUpdateFailedHandler(Exception ex);
/// <summary>
/// Storage progression handler
/// </summary>
/// <param name="progress"></param>
public delegate void StorageProgressHandler(StorageProgressionEventArgs progress);

#endregion

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
public delegate void NetworkGroupSenderHandler(NetPacket packet);
/// <summary>
/// Network user datagram added handler
/// </summary>
/// <param name="connection"></param>
public delegate void NetworkUdpConnectionAddedHandler(NetUdpConnection connection);

#endregion
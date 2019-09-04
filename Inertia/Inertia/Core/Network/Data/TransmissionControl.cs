using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Inertia
{
    public static class TransmissionControl
    {
        public class Client
        {
            public event ClientConnectedHandler OnConnected = () => { };
            public event ClientDisconnectedHandler OnDisconnected = (reason) => { };

            public readonly ClientBase client;

            private Socket socket;
            private readonly byte[] buffer;

            internal Client(ClientBase client)
            {
                buffer = new byte[NetworkModule.BufferLength];
                this.client = client;
            }

            public void Connect(string ip, int port)
            {
                try
                {
                    if (socket != null && socket.Connected)
                        Disconnect(DisconnectReason.ChangeConnection);

                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                    socket.Connect(new IPEndPoint(IPAddress.Parse(ip), port));
                    socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(Receive), socket);
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                    Disconnect(DisconnectReason.CantConnect);
                }

                if (socket.Connected)
                    OnConnected();
            }
            public void Disconnect(DisconnectReason reason)
            {
                try
                {
                    if (socket != null && socket.Connected)
                    {
                        socket.Shutdown(SocketShutdown.Both);
                        socket.Disconnect(false);
                        socket.Close();
                    }
                }
                catch { }

                if (reason != DisconnectReason.ChangeConnection)
                    OnDisconnected(reason);
            }

            private void Receive(IAsyncResult iar)
            {
                try
                {
                    int received = socket.EndReceive(iar);
                    if (received == 0)
                        throw new SocketException();

                    var data = new byte[received];
                    Array.Copy(buffer, data, received);
                    client.Executor.Enqueue(data);
                    Array.Clear(buffer, 0, received);
                }
                catch (Exception e)
                {
                    if (e is SocketException || e is ObjectDisposedException)
                        Disconnect(DisconnectReason.ConnectionLost);
                    else
                        Logger.Error(e);
                }

                if (socket != null && socket.Connected)
                    socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(Receive), socket);
            }

            public void Send(byte[] data)
            {
                var asyncArgs = new SocketAsyncEventArgs();
                asyncArgs.SetBuffer(data, 0, data.Length);
                socket.SendAsync(asyncArgs);
            }
            public void Send<T>(params object[] values) where T : Message
            {
                Send(Message.Build<T>(values));
            }
        }
        public class Server
        {
            public event ServerStartedHandler OnStarted = () => { };
            public event ServerStoppedHandler OnStopped = () => { };
            public event ServerTcpUserStateChangeHandler OnClientJoin = (user) => { };
            public event ServerTcpUserDisconnectedHandler OnClientLeave = (user, reason) => { };

            public ServerState State { get; private set; } = ServerState.Stopped;

            private Socket socket;
            private readonly ServerBase server;
            private readonly int maxQueue;

            internal Server(ServerBase server, int maxQueue)
            {
                this.server = server;
                this.maxQueue = maxQueue;
            }

            public void Start(int port)
            {
                try
                {
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                    socket.Bind(new IPEndPoint(IPAddress.Any, port));
                    socket.Listen(maxQueue);

                    socket.BeginAccept(new AsyncCallback(OnAcceptConnection), socket);

                    OnStarted();
                    State = ServerState.Started;
                }
                catch (SocketException e)
                {
                    Logger.Error(e);
                    Stop();
                }
            }
            public void Stop()
            {
                if (State == ServerState.Started)
                {
                    try
                    {
                        socket?.Shutdown(SocketShutdown.Both);
                        socket?.Disconnect(false);
                    }
                    catch { }
                    socket?.Close();

                    OnStopped();
                    State = ServerState.Stopped;
                }
            }

            private void OnAcceptConnection(IAsyncResult iar)
            {
                try
                {
                    var socket = ((Socket)iar.AsyncState).EndAccept(iar);
                    var user = new UserTransmissionControl(server, socket, NetworkModule.GiveId());

                    OnClientJoin(user);
                }
                catch (Exception) { }

                socket.BeginAccept(new AsyncCallback(OnAcceptConnection), socket);
            }

            internal void OnClientLeaved(User user, DisconnectReason reason)
            {
                OnClientLeave(user, reason);
            }
        }
        public abstract class User
        {
            public readonly ServerBase Server;
            public readonly Socket Socket;
            public readonly uint userId;

            internal readonly Reader Reader;

            private readonly byte[] buffer;

            internal User(ServerBase Server, Socket Socket, uint userId)
            {
                this.Server = Server;
                this.Socket = Socket;
                this.userId = userId;

                buffer = new byte[NetworkModule.BufferLength];
                Reader = new Reader();

                Socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(Receive), Socket);
            }

            private void Receive(IAsyncResult IAR)
            {
                try
                {
                    var received = Socket.EndReceive(IAR);
                    if (received == 0)
                        throw new SocketException();

                    var data = new byte[received];
                    Array.Copy(buffer, data, received);
                    Server.Executor.Enqueue(new ServerGrossPacket(this, data));
                    Array.Clear(buffer, 0, received);

                    if (Socket != null && Socket.Connected)
                        Socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(Receive), Socket);
                }
                catch (Exception e)
                {
                    if (e is SocketException || e is ObjectDisposedException)
                    {
                        Disconnect(DisconnectReason.ConnectionLost);
                        return;
                    }
                }
            }

            public void Disconnect(DisconnectReason reason)
            {
                try
                {
                    if (Socket != null)
                        Socket.Close();
                }
                catch { }

                Server.Tcp.OnClientLeaved(this, reason);
            }

            public void Send(byte[] data)
            {
                var asyncArgs = new SocketAsyncEventArgs();
                asyncArgs.SetBuffer(data, 0, data.Length);
                Socket.SendAsync(asyncArgs);
            }
            public void Send(Type packetType, params object[] values)
            {
                Send(Message.Build(NetworkModule.Module.GetMessage(packetType), values));
            }
            public void Send<T>(params object[] values) where T : Message
            {
                Send(Message.Build<T>(values));
            }
        }
    }
}

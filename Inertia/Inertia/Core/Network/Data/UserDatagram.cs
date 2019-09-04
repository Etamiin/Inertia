using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Inertia
{
    public static class UserDatagram
    {
        public class Client
        {
            public event ClientConnectedHandler OnConnected = () => { };
            public event ClientDisconnectedHandler OnDisconnected = (reason) => { };

            public readonly ClientBase client;

            private UdpClient socket;

            internal Client(ClientBase client)
            {
                this.client = client;
            }

            public void Connect(string ip, int port)
            {
                try
                {
                    if (socket != null && socket.Client != null && socket.Client.Connected)
                        Disconnect(DisconnectReason.ChangeConnection);

                    socket = new UdpClient();
                    socket.Connect(new IPEndPoint(IPAddress.Parse(ip), port));
                    socket.BeginReceive(new AsyncCallback(Receive), socket);

                    OnConnected();
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                    Disconnect(DisconnectReason.CantConnect);
                }
            }
            public void Disconnect(DisconnectReason reason)
            {
                try
                {
                    if (socket != null)
                        socket.Client.Close();

                    socket.Dispose();
                }
                catch { }

                if (reason != DisconnectReason.ChangeConnection)
                    OnDisconnected(reason);
            }

            private void Receive(IAsyncResult iar)
            {
                try
                {
                    IPEndPoint endPoint = null;
                    var data = socket.EndReceive(iar, ref endPoint);
                    client.Executor.Enqueue(data);
                }
                catch { }

                socket.BeginReceive(new AsyncCallback(Receive), socket);
            }

            public void Send(byte[] data)
            {
                socket.SendAsync(data, data.Length);
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
            public event ServerUdpUserStateChangeHandler OnClientJoin = (user) => { };

            public ServerState State { get; private set; } = ServerState.Stopped;

            private UdpClient socket;
            private ServerBase server;
            private Dictionary<IPEndPoint, DatagramUser> users;
            
            internal Server(ServerBase server)
            {
                this.server = server;
                users = new Dictionary<IPEndPoint, DatagramUser>();
            }

            public void Start(int port)
            {
                try
                {
                    socket = new UdpClient(new IPEndPoint(IPAddress.Any, port));
                    socket.BeginReceive(new AsyncCallback(Receive), socket);

                    OnStarted();
                    State = ServerState.Started;
                }
                catch (Exception e)
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
                        socket?.Client?.Close();
                        socket = null;
                    }
                    catch { }

                    OnStopped();
                    State = ServerState.Stopped;
                }
            }

            private void Receive(IAsyncResult iar)
            {
                try
                {
                    IPEndPoint endPoint = null;
                    var data = socket.EndReceive(iar, ref endPoint);
                    DatagramUser user = null;

                    if (!users.ContainsKey(endPoint))
                    {
                        user = new DatagramUser(server, endPoint, NetworkModule.GiveId());
                        users.Add(endPoint, user);

                        OnClientJoin(user);
                    }
                    else
                        user = users[endPoint];

                    server.Executor.Enqueue(new ServerGrossPacket(user, data));
                }
                catch { }

                socket.BeginReceive(new AsyncCallback(Receive), socket);
            }

            internal void SendTo(byte[] data, IPEndPoint endPoint)
            {
                socket.SendAsync(data, data.Length, endPoint);
            }
        }
        public abstract class User
        {
            public readonly ServerBase Server;
            public readonly IPEndPoint EndPoint;
            public readonly uint userId;

            internal readonly Reader Reader;

            internal User(ServerBase Server, IPEndPoint EndPoint, uint userId)
            {
                this.Server = Server;
                this.EndPoint = EndPoint;
                this.userId = userId;

                Reader = new Reader();
            }

            public void Send(byte[] data)
            {
                Server.Udp.SendTo(data, EndPoint);
            }
            public void Send<T>(params object[] values) where T : Message
            {
                Send(Message.Build<T>(values));
            }
        }
    }
}

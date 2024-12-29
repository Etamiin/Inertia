using Inertia.Logging;
using System;
using System.Diagnostics;
using System.Net.Sockets;

namespace Inertia.Network
{
    public class TcpConnectionEntity : NetworkConnectionEntity, IDisposable
    {
        internal event EventHandler<ConnectionDisconnectingArgs>? Disconnecting;

        public bool IsDisposed { get; private set; }
        public bool IsConnected => _socket?.Connected == true;
        public NetworkConnectionMonitoring Monitoring { get; private set; }

        private protected Socket _socket { get; private set; }
        private protected DataReader _networkDataReader { get; private set; }
        private protected byte[] _buffer { get; private set; }

        internal TcpConnectionEntity(Socket socket, uint id, NetworkEntityParameters parameters) : base(id, parameters)
        {
            _socket = socket;
            _networkDataReader = new DataReader();
            _buffer = new byte[NetworkProtocolManager.CurrentProtocol.NetworkBufferLength];
            Monitoring = new NetworkConnectionMonitoring();
        }

        internal protected virtual void BeginReceiveMessages()
        {
            _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, OnReceiveData, _socket);
        }

        public sealed override void Send(byte[] dataToSend)
        {
            this.ThrowIfDisposable(IsDisposed);

            if (IsConnected)
            {
                try
                {
                    ProcessSend(dataToSend);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, GetType(), nameof(Send));
                    Disconnect(NetworkDisconnectReason.InvalidMessageSended);
                }
            }
        }
        public override bool Disconnect(NetworkDisconnectReason reason)
        {
            this.ThrowIfDisposable(IsDisposed);

            if (_socket != null)
            {
                Disconnecting?.Invoke(this, new ConnectionDisconnectingArgs(this, reason));
                Disconnecting = null;

                if (IsConnected)
                {
                    _socket?.Disconnect(false);
                }

                _networkDataReader?.Dispose();
                _networkDataReader = null;
                _socket = null;
                _buffer = null;
            }

            ProcessClean();
            return true;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private protected virtual void ProcessSend(byte[] data)
        {
            _socket.Send(data);
        }
        private protected virtual void ProcessClean() { }
        private protected void ProcessReceivedData(int receivedLength)
        {
            if (!IsConnected) return;
            if (receivedLength == 0) return;

            Monitoring.NotifyMessageReceived();
            if (Monitoring.MessageReceivedInLastSecond >= _parameters.MessageCountLimitBeforeSpam)
            {
                Disconnect(NetworkDisconnectReason.Spam);
                return;
            }
            
            var data = new byte[receivedLength];
            Array.Copy(_buffer, data, receivedLength);

            NetworkProtocolManager.ProcessParsing(this, _networkDataReader.Fill(data));
        }

        private void OnReceiveData(IAsyncResult iar)
        {
            try
            {
                if (!IsConnected) return;

                var received = ((Socket)iar.AsyncState).EndReceive(iar);
                if (received == 0 && !IsDisposed)
                {
                    Disconnect(NetworkDisconnectReason.ConnectionLost);
                    return;
                }

                ProcessReceivedData(received);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, GetType(), nameof(OnReceiveData));

                if (ex is SocketException || ex is ObjectDisposedException)
                {
                    if (!IsDisposed)
                    {
                        Disconnect(NetworkDisconnectReason.ConnectionLost);
                        return;
                    }
                }
            }

            if (IsConnected)
            {
                _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, OnReceiveData, _socket);
            }
        }
        private void Dispose(bool disposing)
        {
            if (!IsDisposed && disposing)
            {
                Disconnect();

                IsDisposed = true;
            }
        }
    }
}
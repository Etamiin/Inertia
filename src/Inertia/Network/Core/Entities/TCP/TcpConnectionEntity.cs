using Inertia.IO;
using Inertia.Logging;
using System;
using System.Net.Sockets;

namespace Inertia.Network
{
    public class TcpConnectionEntity : NetworkConnectionEntity, IDisposable
    {
        internal event EventHandler<ConnectionDisconnectingArgs>? Disconnecting;

        public bool IsDisposed { get; private set; }
        public bool IsConnected => _socket != null && _socket.Connected;
        public NetworkConnectionMonitoring Monitoring { get; private set; }

        private protected Socket _socket { get; private set; }
        private protected DataReader _networkDataReader { get; private set; }
        private protected byte[] _buffer { get; private set; }

        internal TcpConnectionEntity(Socket socket, uint id, NetworkEntityParameters parameters) : base(id, parameters)
        {
            _socket = socket;
            _networkDataReader = new DataReader();
            _buffer = new byte[_parameters.Protocol.NetworkBufferLength];
            Monitoring = new NetworkConnectionMonitoring();
        }

        public bool Disconnect()
        {
            return Disconnect(NetworkDisconnectReason.Manual);
        }
        public void Dispose()
        {
            Dispose(true);
        }

        public sealed override void Send(byte[] dataToSend)
        {
            this.ThrowIfDisposable(IsDisposed);

            if (dataToSend == null || dataToSend.Length == 0)
            {
                throw new ArgumentNullException(nameof(dataToSend));
            }

            if (IsConnected)
            {
                try
                {
                    ProcessSend(dataToSend);
                }
                catch
                {
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

        internal protected virtual void BeginReceiveMessages()
        {
            _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, OnReceiveData, _socket);
        }
        
        private protected virtual void ProcessSend(byte[] data)
        {
            _socket.Send(data);
        }
        private protected virtual void ProcessClean() { }
        private protected void ProcessReceivedData(int receivedLength)
        {
            if (!IsConnected) return;
            if (receivedLength == 0)
            {
                throw new SocketException((int)SocketError.SocketError);
            }

            Monitoring.NotifyMessageReceived();
            if (Monitoring.MessageReceivedInLastSecond >= _parameters.MessageCountLimitBeforeSpam)
            {
                Disconnect(NetworkDisconnectReason.Spam);
                return;
            }
            
            var data = new byte[receivedLength];
            Array.Copy(_buffer, data, receivedLength);

            NetworkProtocolManager.ProcessParsing(_parameters.Protocol, this, _networkDataReader.Fill(data));
        }

        private void OnReceiveData(IAsyncResult iar)
        {
            try
            {
                var received = ((Socket)iar.AsyncState).EndReceive(iar);
                ProcessReceivedData(received);
            }
            catch (Exception ex)
            {
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
            if (IsDisposed) return;

            if (disposing)
            {
                Disconnect();
            }

            IsDisposed = true;
        }
    }
}
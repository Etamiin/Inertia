using Inertia.IO;
using Inertia.Logging;
using System;
using System.Net.Sockets;

namespace Inertia.Network
{
    public abstract class TcpConnectionEntityBase : ConnectionEntity
    {
        internal TcpConnectionEntityBase(Socket socket, uint id, NetworkProtocol networkProtocol) : base(socket, id, networkProtocol)
        {
        }

        internal override void BeginReceiveMessages()
        {
            _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, OnReceiveData, _socket);
        }

        private protected override void DoSend(byte[] data)
        {
            _socket.Send(data);
        }
        private protected void ProcessReceivedData(int receivedLength)
        {
            if (!IsConnected || receivedLength == 0) return;

            Monitoring.NotifyMessageReceived();
            if (Monitoring.MessageReceivedInLastSecond >= NetworkProtocol.MaxReceivedMessagePerSecondPerClient)
            {
                Disconnect(NetworkStopReason.Spam);
                return;
            }

            _safeNetworkDataReaderAccess.Lock((reader) =>
            {
                reader.Fill(_buffer, receivedLength);
            });
            
            ProcessingQueue.Enqueue(() => _safeNetworkDataReaderAccess.Lock((reader) =>
            {
                NetworkManager.ParseAndHandle(this, reader);
            }));
        }

        private void OnReceiveData(IAsyncResult iar)
        {
            try
            {
                if (!IsConnected) return;

                var receivedLength = ((Socket)iar.AsyncState).EndReceive(iar);
                if (receivedLength == 0 && !IsDisposed)
                {
                    Disconnect(NetworkStopReason.ConnectionLost);
                    return;
                }

                ProcessReceivedData(receivedLength);
            }
            catch (Exception ex)
            {
                LoggingProvider.LogHandler.Log(LogLevel.Error, $"An error occurred when receiving socket data", ex);

                if (ex is SocketException || ex is ObjectDisposedException)
                {
                    if (!IsDisposed)
                    {
                        Disconnect(NetworkStopReason.ConnectionLost);
                        return;
                    }
                }
            }

            if (IsConnected)
            {
                _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, OnReceiveData, _socket);
            }
        }
    }
}

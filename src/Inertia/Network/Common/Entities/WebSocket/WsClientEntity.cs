using System;
using System.Threading;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Network
{
    public abstract class WsClientEntity : NetworkClientEntity
    {
        public override bool IsConnected
        {
            get
            {
                return _ws.State == WebSocketState.Open;
            }
        }

        private ClientWebSocket _ws;
        private BasicReader _reader;
        private readonly byte[] _buffer;
        private int _connectionTimeout;

        public WsClientEntity(string ip, int port) : this(ip, port, 10000)
        {
        }
        public WsClientEntity(string ip, int port, int connectionTimeout) : base(ip, port)
        {
            var bufferLength = NetworkProtocol.UsedProtocol.NetworkBufferLength;

            _ws = new ClientWebSocket();
            _ws.Options.SetBuffer(bufferLength, bufferLength);

            _reader = new BasicReader();
            _buffer = new byte[bufferLength];
            _connectionTimeout = connectionTimeout;
        }

        public sealed override void Connect()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(TcpClientEntity));
            }

            _ws.ConnectAsync(new Uri($"ws://{_targetIp}:{_targetPort}"), new CancellationToken());

            var c = new Clock();
            while (_ws.State != WebSocketState.Open)
            {
                Thread.Sleep(30);

                if (c.GetElapsedMilliseconds() >= _connectionTimeout)
                {
                    Disconnect(NetworkDisconnectReason.ConnectionFailed);
                    return;
                }
            }

            OnConnected();
            StartReception();
        }
        public sealed override void Disconnect(NetworkDisconnectReason reason)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(TcpClientEntity));
            }

            if (!IsConnected) return;

            _ws.Abort();
            OnDisconnected(reason);
        }
        public sealed override void Send(byte[] data)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(TcpClientEntity));
            }

            try { _ws.SendAsync(data, WebSocketMessageType.Binary, true, new CancellationToken()); } catch { }
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                Disconnect(NetworkDisconnectReason.Manual);
                
                _ws.Dispose();

                IsDisposed = true;
            }
        }

        private void StartReception()
        {
            var cancelToken = new CancellationToken();
            Task.Factory.StartNew(async () => {
                try
                {
                    while (IsConnected && !cancelToken.IsCancellationRequested)
                    {
                        var result = await _ws.ReceiveAsync(_buffer, cancelToken);
                        NetworkProtocol.ProcessParsing(this, _reader.Fill(new ReadOnlySpan<byte>(_buffer, 0, result.Count)));
                    }
                }
                catch (Exception e)
                {
                    if (e is ObjectDisposedException)
                    {
                        Disconnect(NetworkDisconnectReason.ConnectionLost);
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }
    }
}

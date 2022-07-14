using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Inertia.Network
{
    public sealed class TcpConnectionEntity : NetworkConnectionEntity, IDisposable
    {
        internal event BasicAction<NetworkDisconnectReason> Disconnected;

        public ConnectionType ConnectionType { get; private set; }

        public bool IsConnected => _socket != null && _socket.Connected;
        
        private TcpServerEntity _server;
        private Socket _socket;
        private readonly BasicReader _reader;
        private byte[] _buffer;
        private bool _disconnectionNotified;
        private DateTime _spamTimer;
        private int _dataCountReceivedInLastSecond;

        internal TcpConnectionEntity(TcpServerEntity server, Socket socket, uint id) : base(id)
        {
            _server = server;
            _socket = socket;
            _buffer = new byte[NetworkProtocol.UsedProtocol.NetworkBufferLength];
            _reader = new BasicReader();
        }

        internal void StartReception()
        {
            if (!IsDisposed)
            {
                _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(OnReceiveData), _socket);
            }
        }

        public override void Send(byte[] data)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(TcpConnectionEntity));
            }
            
            if (!IsConnected) return;

            try {
                if (ConnectionType != ConnectionType.WebSocket)
                {
                    _socket.Send(data);
                }
                else
                {
                    _socket.Send(Mask(data));
                }

            } catch { }
        }
        public override void Send(NetworkMessage message)
        {
            Send(NetworkProtocol.UsedProtocol.OnSerializeMessage(message));
        }

        public void Disconnect()
        {
            Disconnect(NetworkDisconnectReason.Manual);
        }
        public void Disconnect(NetworkDisconnectReason reason)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(TcpConnectionEntity));
            }

            if (IsConnected)
            {
                try
                {
                    _socket.Shutdown(SocketShutdown.Both);
                }
                catch { }

                _reader?.Dispose();
                _socket?.Disconnect(false);
            }

            if (!_disconnectionNotified)
            {
                _disconnectionNotified = true;

                Disconnected?.Invoke(reason);
                _server.ConnectionDisconnected(this, reason);

                _server = null;
                _buffer = null;
                _socket = null;
                Disconnected = null;
            }
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                Disconnect();
                IsDisposed = true;
            }
        }

        private void OnReceiveData(IAsyncResult iar)
        {
            try
            {
                var received = ((Socket)iar.AsyncState).EndReceive(iar);
                if (received == 0)
                {
                    throw new SocketException();
                }

                _dataCountReceivedInLastSecond++;
                if (_spamTimer != null)
                {
                    var ts = DateTime.Now - _spamTimer;
                    if (ts.TotalSeconds > 1)
                    {
                        if (_dataCountReceivedInLastSecond >= NetworkProtocol.UsedProtocol.AuthorizedDataCountPerSecond)
                        {
                            Disconnect(NetworkDisconnectReason.SpammingMessages);
                            return;
                        }
                        else
                        {
                            _dataCountReceivedInLastSecond = 0;
                            _spamTimer = DateTime.Now;
                        }
                    }
                }

                if (ConnectionType == ConnectionType.NotDefined)
                {
                    ConnectionType = _server.VerifyConnectionType(this, _buffer, received);
                    if (ConnectionType == ConnectionType.Classic) CallParsing();
                }
                else CallParsing();

                void CallParsing()
                {
                    if (ConnectionType == ConnectionType.WebSocket)
                    {
                        var data = new byte[received];
                        Array.Copy(_buffer, 0, data, 0, received);

                        var decryptedData = Unmask(data);
                        NetworkProtocol.ProcessParsing(this, _reader.Fill(decryptedData));
                    }
                    else
                    {
                        NetworkProtocol.ProcessParsing(this, _reader.Fill(new ReadOnlySpan<byte>(_buffer, 0, received)));
                    }                    
                }                
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
                _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(OnReceiveData), _socket);
            }
        }

        private static byte[] Unmask(byte[] data)
        {
            using (var r = new BasicReader(data))
            {
                while (r.UnreadedLength > 0)
                {
                    var opCodeMask = r.GetByte().ToBits(8);
                    var end = opCodeMask[0];

                    for (var i = 0; i < 4; i++)
                    {
                        opCodeMask[i] = false;
                    }

                    var opCode = opCodeMask.ToByte();
                    var mask = r.GetByte().ToBits(8);
                    var isMasked = mask[0];
                    mask[0] = false;
                    var payloadLength = (int)mask.ToByte();

                    if (payloadLength == 126)
                    {
                        payloadLength = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(r.GetShort());
                    }
                    else if (payloadLength == 127)
                    {
                        payloadLength = (int)System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(r.GetLong());
                    }

                    byte[]? encryptedData;
                    if (isMasked)
                    {
                        if (r.UnreadedLength < payloadLength + 4) break;

                        var maskingKey = r.GetBytes(4);
                        encryptedData = r.GetBytes(payloadLength);
                        for (var i = 0; i < encryptedData.Length; i++)
                        {
                            encryptedData[i] = (byte)(encryptedData[i] ^ maskingKey[i % 4]);
                        }
                    }
                    else
                    {
                        if (r.UnreadedLength < payloadLength) break;

                        encryptedData = r.GetBytes(payloadLength);
                    }

                    return encryptedData;
                }

                return null;
            }
        }
        private static byte[] Mask(byte[] data)
        {
            using (var writer = new BasicWriter())
            {
                writer.SetBoolFlag(true, false, false, false, false, false, true, false);

                var cPos = writer.GetPosition();
                var payloadLength = (byte)
                    (data.Length > 125 && data.Length <= 65536 ? 126 :
                    data.Length > 65536 ? 127 : data.Length);

                var maskBits = payloadLength.ToBits(8);
                maskBits[0] = false;

                writer.SetBoolFlag(maskBits);

                if (payloadLength == 126)
                {
                    var shortLength = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness((short)data.Length);
                    writer.SetShort(shortLength);
                }
                else if (payloadLength == 127)
                {
                    var longLength = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness((long)data.Length);
                    writer.SetLong(longLength);
                }

                writer.SetBytesWithoutHeader(data);
                return writer.ToArray();
            }
        }
    }
}
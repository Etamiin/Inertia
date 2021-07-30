using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inertia;
using Inertia.Network;

namespace InertiaTests.Network
{
    public class CustomProtocol : NetworkProtocol
    {
        public override ushort ProtocolVersion => 64;

        public CustomProtocol()
        {
        }

        public override byte[] OnParseMessage(NetworkMessage message)
        {
            using (BasicWriter writer = new BasicWriter())
            {
                writer
                    .SetUShort(ProtocolVersion)
                    .SetString($"CustomProtocol:{ ProtocolVersion }")
                    .SetUInt(message.Id);

                using (BasicWriter msgWriter = new BasicWriter())
                {
                    message.OnSerialize(msgWriter);

                    writer
                        .SetLong(msgWriter.TotalLength)
                        .SetBytesWithoutHeader(msgWriter.ToArray());
                }

                return writer.ToArray();
            }
        }
        public override void OnReceiveData(TcpClientEntity client, BasicReader reader)
        {
            DefaultReadData(reader, (msg) => GetCaller(msg).TryCall(msg, client));
        }
        public override void OnReceiveData(TcpConnectionEntity connection, BasicReader reader)
        {
            DefaultReadData(reader, (msg) => GetCaller(msg).TryCall(msg, connection));
        }
        public override void OnReceiveData(UdpClientEntity client, BasicReader reader)
        {
            DefaultReadData(reader, (msg) => GetCaller(msg).TryCall(msg, client));
        }
        public override void OnReceiveData(UdpConnectionEntity connection, BasicReader reader)
        {
            DefaultReadData(reader, (msg) => GetCaller(msg).TryCall(msg, connection));
        }

        private void DefaultReadData(BasicReader reader, BasicAction<NetworkMessage> onMessage)
        {
            while (reader.UnreadedLength > 0)
            {
                reader.Position = 0;

                var pVersion = reader.GetUShort();
                var strConfirmVersion = reader.GetString().Split(':');
                var msgId = reader.GetUInt();

                if (strConfirmVersion.Length < 2 || strConfirmVersion[1] != pVersion.ToString())
                    throw new Exception("Invalid ProtocolVersion");

                var msg = CreateMessage(msgId);
                if (msg == null)
                    throw new UnknownMessageException(msgId);

                var msgSize = reader.GetLong();
                if (reader.UnreadedLength < msgSize)
                    break;

                msg.OnDeserialize(reader);
                onMessage(msg);

                reader.RemoveReadedBytes();
            }
        }
    }
}

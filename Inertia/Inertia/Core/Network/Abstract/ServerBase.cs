using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Inertia
{
    public abstract class ServerBase
    {
        public readonly TransmissionControl.Server Tcp;
        public readonly UserDatagram.Server Udp;

        internal readonly QueueExecutor.Auto<ServerGrossPacket> Executor;

        public ServerBase()
        {
            Tcp = new TransmissionControl.Server(this, 1000);
            Udp = new UserDatagram.Server(this);

            Executor = new QueueExecutor.Auto<ServerGrossPacket>((packet) => {
                packet.Reader.Fill(packet.Data);
                packet.Reader.CurrentPosition = 0;

                while (!packet.Reader.IsReaded)
                {
                    if (!Message.Read(packet.Reader, out Reader contentReader, out Message message))
                        break;

                    if (packet.Tcp != null)
                        message.OnReceivedFromServer(packet.Tcp, contentReader);
                    else
                        message.OnReceivedFromServer(packet.Udp, contentReader);

                    if (packet.Reader.LengthAvailable == 0)
                        packet.Reader.Clear();
                }
            });
        }
    }
}
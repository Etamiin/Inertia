using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Inertia
{
    public abstract class ClientBase
    {
        public readonly TransmissionControl.Client Tcp;
        public readonly UserDatagram.Client Udp;

        private readonly Reader Reader;
        internal readonly QueueExecutor.Auto<byte[]> Executor;

        public ClientBase() : base()
        {
            Tcp = new TransmissionControl.Client(this);
            Udp = new UserDatagram.Client(this);

            Reader = new Reader();
            Executor = new QueueExecutor.Auto<byte[]>((data) => {
                Reader.Fill(data);
                Reader.CurrentPosition = 0;

                while (!Reader.IsReaded)
                {
                    if (!Message.Read(Reader, out Reader contentReader, out Message message))
                        break;

                    message.ClientReceivedHandler(this, contentReader);
                    if (Reader.LengthAvailable == 0)
                        Reader.Clear();
                }
            });
        }
    }
}

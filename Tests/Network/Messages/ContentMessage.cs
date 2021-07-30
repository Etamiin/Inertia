using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inertia;
using Inertia.Network;

namespace InertiaTests.Network
{
    public class ContentMessage : NetworkMessage
    {
        public override uint Id => 1;

        public string content;

        public ContentMessage(string content)
        {
            this.content = content;
        }

        public override void OnSerialize(BasicWriter writer)
        {
            writer.SetString(content);
        }
        public override void OnDeserialize(BasicReader reader)
        {
            content = reader.GetString();
        }

        public static void OnReceive(ContentMessage message, TcpConnectionEntity connection)
        {
            Console.WriteLine("Received by TCP server >> " + message.content);
        }
        public static void OnReceive(ContentMessage message, TcpClientEntity client)
        {
            Console.WriteLine("Received by TCP client >> " + message.content);
        }
        public static void OnReceive(ContentMessage message, UdpConnectionEntity connection)
        {
            Console.WriteLine("Received by UDP server >> " + message.content);
        }
        public static void OnReceive(ContentMessage message, UdpClientEntity client)
        {
            Console.WriteLine("Received by UDP client >> " + message.content);
        }
    }
}

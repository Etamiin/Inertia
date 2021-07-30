using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inertia;
using Inertia.Network;

namespace InertiaTests.Network
{
    public class InertiaTestNetwork
    {

        public InertiaTestNetwork()
        {
            NetworkProtocol.SetProtocol(new CustomProtocol());

            Console.WriteLine("-- TCP --");
            Tcp();

            System.Threading.Thread.Sleep(2500);

            Console.WriteLine("-- UDP --");
            Udp();
        }

        private void Tcp()
        {
            var server = new TcpServerEntity("localhost", 7101);

            server.Started += () => Console.WriteLine("TCP Server started");
            server.ClientConnected += (connection) => {
                Console.WriteLine("New connection on server");
                connection.Send(new ContentMessage("Hello from TCP server!"));
            };
            server.ClientDisconnected += (connection, reason) => {
                Console.WriteLine($"Client disconnected: { reason }");
            };

            server.StartAsync();

            var client = new TcpClientEntity("localhost", 7101);

            client.Connected += () => {
                Console.WriteLine("TCP Client connected!");
                client.Send(new ContentMessage("Hello from TCP client!"));
            };
            client.Disconnected += (reason) => Console.WriteLine($"TCP Client disconnected: { reason }");

            client.ConnectAsync();

            System.Threading.Thread.Sleep(1000);
            client.Disconnect();
            server.Close();
        }
        private void Udp()
        {
            var server = new UdpServerEntity("localhost", 7100);

            server.Started += () => Console.WriteLine("UDP Server started");
            server.ConnectionAdded += (connection) => {
                Console.WriteLine("New UDP connection added");
                connection.Send(new ContentMessage("Hello from UDP server!"));
            };

            server.StartAsync();

            var client = new UdpClientEntity("localhost", 7100);

            client.Connected += () => {
                Console.WriteLine("UDP Client connected!");
                client.Send(new ContentMessage("Hello from UDP client!"));
            };
            client.Disconnected += (reason) => Console.WriteLine($"UDP Client disconnected: { reason }");

            client.ConnectAsync();

            System.Threading.Thread.Sleep(1000);
            client.Disconnect();
            server.Close();
        }
    }
}

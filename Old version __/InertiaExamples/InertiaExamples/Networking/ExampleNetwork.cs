using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inertia;
using Inertia.Network;

public class ExampleNetwork
{
    public static ExampleNetwork Instance { get; private set; }

    private NetTcpServer m_server;
    private BaseLogger m_logger;

    public ExampleNetwork()
    {
        if (Instance != null)
            return;

        Instance = this;
        m_logger = Instance.GetLogger();

        //Set custom protocol
        NetworkProtocol.SetProtocol(new CustomNetworkProtocol());

        //Initialize a new TCP server on local IP and PORT 7101
        m_server = new NetTcpServer("127.0.0.1", 7101);

        //Initialize server events
        m_server.Started += OnServerStarted;
        m_server.Closed += (reason) => m_logger.Log("Server closed: " + reason);
        m_server.ClientConnected += OnServerReceiveClient;
        m_server.ClientDisconnected += (client, reason) => m_logger.Log("Client disconnected from server: " + reason);

        //Start the server
        m_server.Start();
        //m_server.StartAsync();
    }

    private void OnServerStarted()
    {
        m_logger.Log("Server started");

        //Create test client
        var client = new NetTcpClient("127.0.0.1", 7101);
        //You can initialize client events here
        client.Disconnected += (reason) => m_logger.Log("Client disconnect: " + reason);
        //Connect the client
        client.Connect();
        //client.ConnectAsync();
    }
    private void OnServerReceiveClient(NetTcpConnection connection)
    {
        m_logger.Log("New client connected to the server");

        //Send a HelloWorldMessage to all new client
        connection.Send(new HelloWorldMessage("Hello this is server."));
    }
}
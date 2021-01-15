using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inertia;
using Inertia.Network;

public class HelloWorldMessage : NetworkMessage
{
    public override uint Id => 1;

    public string message;

    //initialize a new instance of the message
    public HelloWorldMessage(string message)
    {
        this.message = message;
    }

    public override void OnSerialize(BasicWriter writer)
    {
        //serialize message data
        writer.SetString(message);
    }

    public override void OnDeserialize(BasicReader reader)
    {
        //deserialize message data
        message = reader.GetString();
    }

    //Message's hookers can be created in any class
    //Message's hookers need to be a static and public method

    //Define a method for receiving the HelloWorldMessage (for a tcp client)
    public static void OnReceive(HelloWorldMessage helloWorldMessage, NetTcpClient client)
    {
        //Client receive HelloWorldMessage
        client.GetLogger().Log("Client rcv message from server >> " + helloWorldMessage.message);

        //Send a message back to the server
        client.Send(new HelloWorldMessage("Hi server, this is the client!"));

        //disconnect the client for the example
        client.Disconnect(/*NetworkDisconnectReason*/);
    }

    //Define a method for receiving the HelloWorldMessage (for a tcp server)
    public static void OnReceive(HelloWorldMessage helloWorldMessage, NetTcpConnection connection)
    {
        //Server receive HelloWorldMessage
        connection.GetLogger().Log("Server rcv message from client >> " + helloWorldMessage.message);
    }
}
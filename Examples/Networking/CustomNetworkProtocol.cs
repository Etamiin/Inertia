using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inertia;
using Inertia.Network;

public class CustomNetworkProtocol : NetworkProtocol
{
    public override ushort ProtocolVersion => 1;

    public CustomNetworkProtocol()
    {

    }

    //Redefine custom parsing messages
    public override byte[] OnParseMessage(NetworkMessage message)
    {
        using (BasicWriter writer = new BasicWriter())
        {
            //write your custom protocol

            var headerType = 1;

            writer
                .SetInt(headerType)
                .SetUInt(message.Id);

            //serialize the message
            message.OnSerialize(writer);

            return writer.ToArray();
        }
    }

    //Redefine custom method when receiving a TCP message for the TCP clients
    public override void OnReceiveData(NetTcpClient client, BasicReader reader)
    {
        var message = CustomReception(reader);

        //Calling HookerRefs will execute the NetworkMessage reception at that moment in the code
        //You can create an async call by yourself if you don't want this thread to be stopped by an message execution

        //If you want to call multiple hookers:
        /*
        var refs = GetHookerRefs(message);
        if (refs != null)
            refs.CallHookerRef(message, client);
        */

        //If you just want to directly call the hooker (common method):
        CallHookerRef(message, client);
    }

    //Redefine custom method when receiving a TCP message for the TCP server
    public override void OnReceiveData(NetTcpConnection connection, BasicReader reader)
    {
        var message = CustomReception(reader);

        //Calling HookerRefs will execute the NetworkMessage reception at that moment in the code
        //You can create an async call by yourself if you don't want this thread to be stopped by an message execution

        //If you want to call multiple hookers:
        /*
        var refs = GetHookerRefs(message);
        if (refs != null)
            refs.CallHookerRef(message, client);
        */

        //If you just want to directly call the hooker (common method):
        CallHookerRef(message, connection);
    }

    //generic method for all reception (example)
    private NetworkMessage CustomReception(BasicReader reader)
    {
        //read your custom protocol

        var headerType = reader.GetInt();
        var messageId = reader.GetUInt();

        //create NetworkMessage instance based on Type or Id
        var message = CreateInstance(messageId);
        if (message != null)
        {
            //deserialize the message
            message.OnDeserialize(reader);
            return message;
        }
        else
        {
            //MessageID received don't exist
            throw new UnknownMessageException(messageId);
        }
    }
}
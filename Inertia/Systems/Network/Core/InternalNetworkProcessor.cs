using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Inertia.Network;

namespace Inertia.Internal
{
    internal class InternalNetworkProcessor : NetworkProcessor
    {
        #region Static variables

        internal static InternalNetworkProcessor Processor;

        #endregion

        #region Private variables

        private readonly Dictionary<uint, NetworkPacket> Packets;
        private readonly Dictionary<Type, NetworkPacket> PacketTypes;
        private readonly AutoQueue PacketProcessor;
        private readonly AutoQueue PacketExecutor;

        #endregion

        #region Constructors

        public InternalNetworkProcessor(object inProcessor) : base(inProcessor)
        {
            if (Processor != null)
                return;

            Processor = this;
            Packets = new Dictionary<uint, NetworkPacket>();
            PacketTypes = new Dictionary<Type, NetworkPacket>();
            PacketExecutor = new AutoQueue(QueueMod.AlwaysAlive);
            PacketProcessor = new AutoQueue(QueueMod.AlwaysAlive);

            LoadPackets();
        }

        #endregion

        private void LoadPackets()
        {
            var assemblys = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblys)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (type.IsClass && type.IsSubclassOf(typeof(NetworkPacket)))
                    {
                        var packet = (NetworkPacket)type.GetConstructor(Type.EmptyTypes).Invoke(new object[] { });
                        if (Packets.ContainsKey(packet.Id))
                            continue;

                        Packets.Add(packet.Id, packet);
                        PacketTypes.Add(packet.GetType(), packet);
                    }
                }
            }
        }

        public static NetworkPacket GetPacket(Type packetType)
        {
            Processor.PacketTypes.TryGetValue(packetType, out NetworkPacket packet);
            return packet;
        }
        public static NetworkPacket GetPacket(uint packetId)
        {
            Processor.Packets.TryGetValue(packetId, out NetworkPacket packet);
            return packet;
        }
        public static T GetPacket<T>() where T : NetworkPacket
        {
            var packet = GetPacket(typeof(T));
            if (packet != null)
                return (T)packet;
            return default;
        }

        public static void OnReceiveClientData(Client client, byte[] data)
        {
            Processor.PacketProcessor.Enqueue(() => {
                if (!client.IsConnected)
                    return;

                NetworkProcessData dataProcessor;
                if (client is TcpNetworkClient tcpClient)
                    dataProcessor = new NetworkProcessData(client.Protocole, tcpClient.TcpReader, data);
                else
                    dataProcessor = new NetworkProcessData(client.Protocole, data);

                OnReceiveData(ref dataProcessor,
                    () => InertiaConfiguration.NetworkProcessor.ProcessClientData(client, ref dataProcessor),
                    (packet) => packet.Packet.OnClientReceive(client, packet.ContentReader));
            });
        }
        public static void OnReceiveServerData(NetworkUser user, byte[] data)
        {
            Processor.PacketProcessor.Enqueue(() => {
                NetworkProcessData dataProcessor;
                if (user is TcpNetworkUser tcpUser)
                {
                    if (!tcpUser.IsConnected)
                        return;

                    dataProcessor = new NetworkProcessData(user.Protocole, tcpUser.TcpReader, data);
                }
                else
                {
                    if (!((UdpNetworkUser)user).IsInitialized)
                        return;

                    dataProcessor = new NetworkProcessData(user.Protocole, data);
                }

                OnReceiveData(ref dataProcessor,
                    () => InertiaConfiguration.NetworkProcessor.ProcessServerData(user, ref dataProcessor),
                    (packet) => packet.Packet.OnServerReceive(user, packet.ContentReader));
            });
        }

        public override void ProcessClientData(Client client, ref NetworkProcessData data)
        {
            OnProcessData(ref data);
        }
        public override void ProcessServerData(NetworkUser user, ref NetworkProcessData data)
        {
            OnProcessData(ref data);
        }
        public override byte[] BuildClientMessage(NetworkMessage message)
        {
            return OnBuildMessage(message);
        }
        public override byte[] BuildServerMessage(NetworkMessage message)
        {
            return OnBuildMessage(message);
        }

        private static void OnReceiveData(ref NetworkProcessData dataProcessor, InertiaAction processAction, InertiaAction<NetworkDeserializedPacket> executePacketAction)
        {
            dataProcessor.Reader.Position = 0;
            while (dataProcessor.Reader.UnreadedLength > 0)
            {
                processAction();

                if (dataProcessor.IsIncompletedData)
                    break;
                else if (dataProcessor.IsInvalidData)
                    throw new SocketException((int)SocketError.MessageSize);

                dataProcessor.Reader.RemoveReadedBytes();
            }

            if (dataProcessor.HasPackets)
            {
                var packets = dataProcessor.GetPackets();
                for (var i = 0; i < packets.Length; i++)
                {
                    var packet = packets[i];

                    Processor.PacketExecutor.Enqueue(() => {
                        executePacketAction(packet);
                        packet.Dispose();
                    });
                }
            }

            dataProcessor.Dispose();
        }
        private void OnProcessData(ref NetworkProcessData data)
        {
            var packetId = data.Reader.GetUInt();
            var messageSize = data.Reader.GetInt();

            if (data.Reader.UnreadedLength < messageSize)
            {
                data.SetIncomplete();
                return;
            }

            var added = data.EnqueuePacket(packetId, data.Reader.GetBytes(messageSize));
            if (!added)
                data.SetInvalid();
        }
        private byte[] OnBuildMessage(NetworkMessage message)
        {
            var writer = new InertiaWriter()
                .SetUInt(message.Packet.Id)
                .SetInt(message.Length)
                .SetBytesWithoutHeader(message.Export());

            return writer.ExportAndDispose();
        }
    }
}

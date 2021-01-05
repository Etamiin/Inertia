using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Network
{
    /// <summary>
    /// Represent the main <see cref="NetworkProtocol"/> used for internal networking
    /// </summary>
    public class DefaultNetworkProtocol : NetworkProtocol
    {
        #region Static variables

        /// <summary>
        /// Get the instance of <see cref="DefaultNetworkProtocol"/> used
        /// </summary>
        public static DefaultNetworkProtocol Instance
        {
            get
            {
                if (m_instance == null)
                    m_instance = new DefaultNetworkProtocol();

                return m_instance;
            }
        }
        private static DefaultNetworkProtocol m_instance;

        /// <summary>
        /// Set to True if you want the packets to be executed as multithreaded, otherwise False
        /// </summary>
        public static bool MultiThreadedExecution { get; set; } = true;
        /// <summary>
        /// The network buffer length used for communications
        /// </summary>
        public static int NetworkBufferLength = 8192;

        #endregion

        #region Public variables

        /// <summary>
        /// Represent the protocol version used by the current protocol
        /// </summary>
        public override ushort ProtocolVersion => 1;

        #endregion

        #region Private variables

        private readonly AutoQueueExecutor m_queue;

        #endregion

        #region Constructors

        internal DefaultNetworkProtocol()
        {
            m_queue = new AutoQueueExecutor();
        }

        #endregion

        /// <summary>
        /// Happens when parsing a <see cref="NetworkMessage"/> instance
        /// </summary>
        /// <param name="packet">Packet to parse</param>
        /// <returns>Parsed packet to byte array</returns>
        public override byte[] OnParsePacket(NetworkMessage packet)
        {
            using (var packetWriter = new BasicWriter())
            {
                using (var coreWriter = new BasicWriter())
                {
                    coreWriter.SetUInt(packet.Id);
                    packet.OnSerialize(coreWriter);

                    packetWriter
                        .SetUInt((uint)coreWriter.TotalLength)
                        .SetBytesWithoutHeader(coreWriter.ToArrayAndDispose());
                }

                return packetWriter.ToArrayAndDispose();
            }
        }

        /// <summary>
        /// Happens when receiving data from a <see cref="NetTcpClient"/>
        /// </summary>
        /// <param name="client">The connection that received the data</param>
        /// <param name="reader">The data in a <see cref="BasicReader"/> instance</param>
        public override void OnReceiveData(NetTcpClient client, BasicReader reader)
        {
            ParseMessages(reader, (packet) => GetHookerRefs(packet)?.CallHookerRef(packet, client));
        }
        /// <summary>
        /// Happens when receiving data from a <see cref="NetUdpClient"/>
        /// </summary>
        /// <param name="client">The connection that received the data</param>
        /// <param name="reader">The data in a <see cref="BasicReader"/> instance</param>
        public override void OnReceiveData(NetUdpClient client, BasicReader reader)
        {
            ParseMessages(reader, (packet) => GetHookerRefs(packet)?.CallHookerRef(packet, client));
        }
        /// <summary>
        /// Happens when receiving data from a <see cref="NetTcpConnection"/>
        /// </summary>
        /// <param name="connection">The connection that received the data</param>
        /// <param name="reader">The data in a <see cref="BasicReader"/> instance</param>
        public override void OnReceiveData(NetTcpConnection connection, BasicReader reader)
        {
            ParseMessages(reader, (packet) => GetHookerRefs(packet)?.CallHookerRef(packet, connection));
        }
        /// <summary>
        /// Happens when receiving data from a <see cref="NetUdpConnection"/>
        /// </summary>
        /// <param name="connection">The connection that received the data</param>
        /// <param name="reader">The data in a <see cref="BasicReader"/> instance</param>
        public override void OnReceiveData(NetUdpConnection connection, BasicReader reader)
        {
            ParseMessages(reader, (packet) => GetHookerRefs(packet)?.CallHookerRef(packet, connection));
        }

        private void ParseMessages(BasicReader reader, BasicAction<NetworkMessage> onPacketParsed)
        {
            while (reader.UnreadedLength > 0)
            {
                reader.Position = 0;

                var size = reader.GetUInt();
                if (reader.UnreadedLength < size)
                    break;

                try
                {
                    using (var core = new BasicReader(reader.GetBytes(size)))
                    {
                        var packetId = core.GetUInt();
                        if (!MessageTypes.ContainsKey(packetId))
                            throw new UnknownMessageException(packetId);

                        var packet = CreateInstance(MessageTypes[packetId]);
                        packet.OnDeserialize(core);

                        if (MultiThreadedExecution)
                            onPacketParsed(packet);
                        else
                            m_queue.Enqueue(() => onPacketParsed(packet));
                    }
                }
                catch (Exception ex) { this.GetLogger().Log(ex); }

                reader.RemoveReadedBytes();
            }
        }
    }
}

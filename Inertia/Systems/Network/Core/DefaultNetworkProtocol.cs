using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inertia.Network;

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

        #endregion

        #region Private variables

        private readonly Dictionary<uint, Type> m_packets;

        #endregion

        #region Constructors

        internal DefaultNetworkProtocol()
        {
            m_packets = new Dictionary<uint, Type>();

            var assemblys = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblys)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (type.IsClass && type.IsSubclassOf(typeof(NetPacket)))
                    {
                        if (type.IsAbstract)
                            continue;

                        var packet = CreateInstance(type);
                        if (m_packets.ContainsKey(packet.Id))
                            continue;

                        m_packets.Add(packet.Id, type);
                    }
                }
            }

        }

        #endregion

        /// <summary>
        /// Happens when parsing a <see cref="NetPacket"/> instance
        /// </summary>
        /// <param name="packet">Packet to parse</param>
        /// <returns>Parsed packet to byte array</returns>
        public override byte[] OnParsePacket(NetPacket packet)
        {
            var writer = new SimpleWriter();
            var coreWriter = new SimpleWriter()
                .SetObject(packet);

            var core = coreWriter.ToArrayAndDispose();
            var compressed = false;
            if (core.LongLength > byte.MaxValue)
                core = core.Compress(out compressed);

            writer
                .SetBool(false)
                .SetBool(compressed)
                .SetLong(core.LongLength)
                .SetBytesWithoutHeader(core);

            return writer.ToArrayAndDispose();
        }
        /// <summary>
        /// Happens when parsing a <see cref="CustomNetPacket"/> instance
        /// </summary>
        /// <param name="packet">Packet to parse</param>
        /// <returns>Parsed packet to byte array</returns>
        public override byte[] OnParsePacket(CustomNetPacket packet)
        {
            var writer = new SimpleWriter();
            var coreWriter = new SimpleWriter();

            packet.OnSerialize(coreWriter);

            var core = coreWriter.ToArrayAndDispose();
            var compressed = false;
            if (core.LongLength > byte.MaxValue)
                core = core.Compress(out compressed);

            writer
                .SetBool(true)
                .SetBool(compressed)
                .SetLong(core.LongLength)
                .SetBytesWithoutHeader(core)
                .SetUInt(packet.Id);

            return writer.ToArrayAndDispose();
        }

        /// <summary>
        /// Happens when receiving data from a <see cref="NetTcpClient"/>
        /// </summary>
        /// <param name="client">The connection that received the data</param>
        /// <param name="reader">The data in a <see cref="SimpleReader"/> instance</param>
        public override void OnReceiveData(NetTcpClient client, SimpleReader reader)
        {
            ParseMessages(reader, (packet) => packet.OnReceived(client));
        }
        /// <summary>
        /// Happens when receiving data from a <see cref="NetUdpClient"/>
        /// </summary>
        /// <param name="client">The connection that received the data</param>
        /// <param name="reader">The data in a <see cref="SimpleReader"/> instance</param>
        public override void OnReceiveData(NetUdpClient client, SimpleReader reader)
        {
            ParseMessages(reader, (packet) => packet.OnReceived(client));
        }
        /// <summary>
        /// Happens when receiving data from a <see cref="NetTcpConnection"/>
        /// </summary>
        /// <param name="connection">The connection that received the data</param>
        /// <param name="reader">The data in a <see cref="SimpleReader"/> instance</param>
        public override void OnReceiveData(NetTcpConnection connection, SimpleReader reader)
        {
            ParseMessages(reader, (packet) => packet.OnReceived(connection));
        }
        /// <summary>
        /// Happens when receiving data from a <see cref="NetUdpConnection"/>
        /// </summary>
        /// <param name="connection">The connection that received the data</param>
        /// <param name="reader">The data in a <see cref="SimpleReader"/> instance</param>
        public override void OnReceiveData(NetUdpConnection connection, SimpleReader reader)
        {
            ParseMessages(reader, (packet) => packet.OnReceived(connection));
        }

        private NetPacket CreateInstance(Type packetType)
        {
            var constr = packetType.GetConstructors()[0];
            var parameters = constr.GetParameters();
            var objs = new object[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
                objs[i] = null;

            return (NetPacket)constr.Invoke(objs);
        }
        private bool IsSizeComplete(SimpleReader reader, out long size)
        {
            size = reader.GetLong();
            return reader.UnreadedLength >= size;
        }
        private void ParseMessages(SimpleReader reader, SimpleAction<NetPacket> onPacketParsed)
        {
            while (reader.UnreadedLength > 0)
            {
                reader.Position = 0;

                var isCustomPacket = reader.GetBool();
                var isCompressed = reader.GetBool();
                var complete = IsSizeComplete(reader, out long size);
                if (!complete)
                    break;

                var data = reader.GetBytes(size);
                if (isCompressed)
                    data = data.Decompress();

                var coreReader = new SimpleReader(data);

                if (isCustomPacket)
                {
                    var packetId = reader.GetUInt();
                    if (!m_packets.ContainsKey(packetId))
                        throw new Exception();

                    var packet = (CustomNetPacket)CreateInstance(m_packets[packetId]);

                    packet.OnDeserialize(coreReader);
                    onPacketParsed(packet);
                }
                else {
                    var obj = (NetPacket)coreReader.GetObject();
                    if (!m_packets.ContainsKey(obj.Id))
                        throw new Exception();

                    var parsedObj = Convert.ChangeType(obj, m_packets[obj.Id]);
                    onPacketParsed((NetPacket)parsedObj);
                }

                coreReader.Dispose();
                reader.RemoveReadedBytes();
            }
        }
    }
}

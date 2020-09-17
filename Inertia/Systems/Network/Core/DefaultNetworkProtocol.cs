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

        #endregion

        #region Private variables

        private readonly AutoQueue m_queue;

        #endregion

        #region Constructors

        internal DefaultNetworkProtocol()
        {
            m_queue = new AutoQueue();
        }

        #endregion

        /// <summary>
        /// Happens when parsing a <see cref="NetworkMessage"/> instance
        /// </summary>
        /// <param name="packet">Packet to parse</param>
        /// <returns>Parsed packet to byte array</returns>
        public override byte[] OnParsePacket(NetworkMessage packet)
        {
            var writer = new BasicWriter();
            var coreWriter = new BasicWriter();

            packet.OnSerialize(coreWriter);

            var core = coreWriter.ToArrayAndDispose();

            writer
                .SetBool(true)
                .SetLong(core.LongLength)
                .SetBytesWithoutHeader(core)
                .SetUInt(packet.Id);

            return writer.ToArrayAndDispose();
        }

        /// <summary>
        /// Happens when receiving data from a <see cref="NetTcpClient"/>
        /// </summary>
        /// <param name="client">The connection that received the data</param>
        /// <param name="reader">The data in a <see cref="BasicReader"/> instance</param>
        public override void OnReceiveData(NetTcpClient client, BasicReader reader)
        {
            ParseMessages(reader, (packet) => { ExecuteHooker(packet, client); });
        }
        /// <summary>
        /// Happens when receiving data from a <see cref="NetUdpClient"/>
        /// </summary>
        /// <param name="client">The connection that received the data</param>
        /// <param name="reader">The data in a <see cref="BasicReader"/> instance</param>
        public override void OnReceiveData(NetUdpClient client, BasicReader reader)
        {
            ParseMessages(reader, (packet) => ExecuteHooker(packet, client));
        }
        /// <summary>
        /// Happens when receiving data from a <see cref="NetTcpConnection"/>
        /// </summary>
        /// <param name="connection">The connection that received the data</param>
        /// <param name="reader">The data in a <see cref="BasicReader"/> instance</param>
        public override void OnReceiveData(NetTcpConnection connection, BasicReader reader)
        {
            ParseMessages(reader, (packet) => ExecuteHooker(packet, connection));
        }
        /// <summary>
        /// Happens when receiving data from a <see cref="NetUdpConnection"/>
        /// </summary>
        /// <param name="connection">The connection that received the data</param>
        /// <param name="reader">The data in a <see cref="BasicReader"/> instance</param>
        public override void OnReceiveData(NetUdpConnection connection, BasicReader reader)
        {
            ParseMessages(reader, (packet) => ExecuteHooker(packet, connection));
        }

        private bool IsSizeComplete(BasicReader reader, out long size)
        {
            if (reader.UnreadedLength < sizeof(long))
            {
                size = 0;
                return false;
            }

            size = reader.GetLong();
            return reader.UnreadedLength >= size + sizeof(int);
        }
        private void ParseMessages(BasicReader reader, BasicAction<NetworkMessage> onPacketParsed)
        {
            while (reader.UnreadedLength > 0)
            {
                reader.Position = 0;

                var isCustomPacket = reader.GetBool();
                var complete = IsSizeComplete(reader, out long size);
                if (!complete)
                    break;

                var data = reader.GetBytes(size);

                try
                {
                    var coreReader = new BasicReader();

                    if (isCustomPacket)
                    {
                        var packetId = reader.GetUInt();
                        if (!MessageTypes.ContainsKey(packetId))
                            throw new Exception();

                        coreReader.Fill(data);

                        var packet = (NetworkMessage)CreateInstance(MessageTypes[packetId]);
                        packet.OnDeserialize(coreReader);

                        m_queue.Enqueue(() => onPacketParsed(packet));
                    }
                    else
                    {
                        var isCompressed = reader.GetBool();
                        if (isCompressed)
                            data = data.Decompress();

                        var obj = (NetworkMessage)coreReader.GetObject();
                        if (!MessageTypes.ContainsKey(obj.Id))
                            throw new Exception();

                        coreReader.Fill(data);

                        var parsedObj = Convert.ChangeType(obj, MessageTypes[obj.Id]);
                        m_queue.Enqueue(() => onPacketParsed((NetworkMessage)parsedObj));
                    }

                    coreReader.Dispose();
                }
                catch (Exception ex) { BaseLogger.DefaultLogger.Log(ex); }

                reader.RemoveReadedBytes();
            }
        }
    }
}

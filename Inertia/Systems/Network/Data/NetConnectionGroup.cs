using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Network
{
    /// <summary>
    /// Allows sending data to multiple connections 
    /// </summary>
    public class NetConnectionGroup : IDisposable
    {
        #region Events

        private event NetworkGroupSenderHandler Sender = (packet) => { };

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiate a new instance of the class <see cref="NetConnectionGroup"/>
        /// </summary>
        public NetConnectionGroup()
        {
        }

        #endregion

        /// <summary>
        /// Add a <see cref="NetClient"/> to the group
        /// </summary>
        /// <param name="connection">Connection to add</param>
        /// <returns>The current instance</returns>
        public NetConnectionGroup AddConnection(NetClient connection)
        {
            Sender += connection.Send;
            return this;
        }
        /// <summary>
        /// Add a <see cref="NetTcpConnection"/> to the group
        /// </summary>
        /// <param name="connection">Connection to add</param>
        /// <returns>The current instance</returns>
        public NetConnectionGroup AddConnection(NetTcpConnection connection)
        {
            Sender += connection.Send;
            return this;
        }
        /// <summary>
        /// Add a <see cref="NetUdpConnection"/> to the group
        /// </summary>
        /// <param name="connection">Connection to add</param>
        /// <returns>The current instance</returns>
        public NetConnectionGroup AddConnection(NetUdpConnection connection)
        {
            Sender += connection.Send;
            return this;
        }
        /// <summary>
        /// Remove a <see cref="NetClient"/> from the group
        /// </summary>
        /// <param name="connection">Connection to remove</param>
        /// <returns>The current instance</returns>
        public NetConnectionGroup RemoveConnection(NetClient connection)
        {
            Sender -= connection.Send;
            return this;
        }
        /// <summary>
        /// Remove a <see cref="NetTcpConnection"/> from the group
        /// </summary>
        /// <param name="connection">Connection to remove</param>
        /// <returns>The current instance</returns>
        public NetConnectionGroup RemoveConnection(NetTcpConnection connection)
        {
            Sender -= connection.Send;
            return this;
        }
        /// <summary>
        /// Remove a <see cref="NetUdpConnection"/> from the group
        /// </summary>
        /// <param name="connection">Connection to remove</param>
        /// <returns>The current instance</returns>
        public NetConnectionGroup RemoveConnection(NetUdpConnection connection)
        {
            Sender -= connection.Send;
            return this;
        }

        /// <summary>
        /// Send the specified <see cref="NetPacket"/> to the group
        /// </summary>
        /// <param name="packet">Packet instance to send</param>
        public void Send(NetPacket packet)
        {
            if (Sender == null)
                return;

            Sender(packet);
        }

        /// <summary>
        /// Dispose the current instance
        /// </summary>
        public void Dispose()
        {
            Sender = null;
        }
    }
}

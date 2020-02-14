using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inertia.Internal;

namespace Inertia.Network
{
    public class NetworkProcessData : IDisposable
    {
        #region Public variables

        public InertiaReader Reader { get; private set; }
        public NetworkProtocole ReceivedProtocole { get; private set; }
        public bool IsIncompletedData { get; private set; }
        public bool IsInvalidData { get; private set; }
        public bool HasPackets
        {
            get
            {
                return Packets.Count > 0;
            }
        }

        #endregion

        #region Private variables

        private List<NetworkDeserializedPacket> Packets;

        #endregion

        #region Constructors

        internal NetworkProcessData(NetworkProtocole protocole, byte[] data)
        {
            ReceivedProtocole = protocole;
            Reader = new InertiaReader(data);
            Packets = new List<NetworkDeserializedPacket>();
        }
        internal NetworkProcessData(NetworkProtocole protocole, InertiaReader reader, byte[] data)
        {
            ReceivedProtocole = protocole;
            Reader = reader;
            Reader.Fill(data);
            Packets = new List<NetworkDeserializedPacket>();
        }

        #endregion

        internal NetworkDeserializedPacket[] GetPackets()
        {
            return Packets.ToArray();
        }

        public void SetIncomplete()
        {
            IsIncompletedData = true;
        }
        public void SetInvalid()
        {
            IsInvalidData = true;
        }

        public bool EnqueuePacket(uint packetId, byte[] packetContent)
        {
            var packet = InternalNetworkProcessor.GetPacket(packetId);
            if (packet == null)
                return false;

            Packets.Add(new NetworkDeserializedPacket(packet, packetContent));
            return true;
        }

        public void Dispose()
        {
            if (Reader == null)
                return;

            if (ReceivedProtocole == NetworkProtocole.UserDatagram)
                Reader.Dispose();
            else if (ReceivedProtocole == NetworkProtocole.TransmissionControl && Reader.TotalLength == 0)
                Reader.Clear();            
            
            Reader = null;
            Packets?.Clear();
            Packets = null;
        }
    }
}

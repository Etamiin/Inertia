using System;
using System.Collections.Generic;
using System.Text;

namespace Inertia.Network
{
    public sealed class MessageParsingOutput
    {
        internal List<NetworkMessage> Messages { get; private set; }

        internal MessageParsingOutput()
        {
            Messages = new List<NetworkMessage>();
        }

        public void AddMessage(NetworkMessage message)
        {
            Messages.Add(message);
        }

        internal void Clean()
        {
            Messages.Clear();
            Messages = null;
        }
    }
}

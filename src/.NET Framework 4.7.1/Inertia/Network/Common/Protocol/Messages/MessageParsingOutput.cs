using System;
using System.Collections.Generic;
using System.Text;

namespace Inertia.Network
{
    public sealed class MessageParsingOutput
    {
        public bool IsDisposed { get; private set; }

        internal List<NetworkMessage> Messages { get; private set; }

        internal MessageParsingOutput()
        {
            Messages = new List<NetworkMessage>();
        }

        public void AddMessage(NetworkMessage message)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(MessageParsingOutput));
            }

            Messages.Add(message);
        }

        internal void Clean()
        {
            if (!IsDisposed)
            {
                Messages.Clear();
                Messages = null;

                IsDisposed = true;
            }
        }
    }
}

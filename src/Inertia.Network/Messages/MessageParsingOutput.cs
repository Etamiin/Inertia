using System;
using System.Collections.Generic;

namespace Inertia.Network
{
    public sealed class MessageParsingOutput : IDisposable
    {
        internal MessageParsingOutput()
        {
            Messages = new List<NetworkMessage>();
        }

        internal List<NetworkMessage> Messages { get; }

        public bool IsDisposed { get; private set; }
        
        public void AddMessage(NetworkMessage message)
        {
            this.ThrowIfDisposable(IsDisposed);

            Messages.Add(message);
        }
        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (!IsDisposed && disposing)
            {
                Messages.Clear();

                IsDisposed = true;
            }
        }
    }
}

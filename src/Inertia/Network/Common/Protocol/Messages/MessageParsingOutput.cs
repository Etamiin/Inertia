using System;
using System.Collections.Generic;

namespace Inertia.Network
{
    public class MessageParsingOutput : IDisposable
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
        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (IsDisposed) return;

            if (disposing)
            {
                Messages.Clear();
                Messages = null;
            }

            IsDisposed = true;
        }
    }
}

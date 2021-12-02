using System;
using System.Collections.Generic;
using System.Text;

namespace Inertia.Network
{
    public sealed class MessageParsingOutput : IDisposable
    {
        internal List<NetworkMessage> Messages { get; private set; }

        private bool _outputReaded = false;

        internal MessageParsingOutput()
        {
            Messages = new List<NetworkMessage>();
        }

        public void AddOutput(NetworkMessage message)
        {
            Messages.Add(message);
        }

        public void Dispose()
        {
            if (_outputReaded)
            {
                Messages.Clear();
            }
        }

        internal MessageParsingOutput SetAsReaded()
        {
            _outputReaded = true;
            return this;
        }
    }
}

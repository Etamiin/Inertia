using System;

namespace Inertia.Network
{
    public sealed class ConnectionStatistics
    {
        public int MessageReceivedCount { get; private set; }
        public int MessageReceivedInLastSecond { get; private set; }

        private DateTime? _lastSecondTimer;
        
        internal ConnectionStatistics()
        {
        }

        internal void NotifyMessageReceived()
        {
            MessageReceivedCount++;
            MessageReceivedInLastSecond++;

            if (!_lastSecondTimer.HasValue)
            {
                _lastSecondTimer = DateTime.Now;
            }
            else
            {
                var span = DateTime.Now - _lastSecondTimer.Value;
                if (span.TotalSeconds >= 1)
                {
                    MessageReceivedInLastSecond = 0;
                    _lastSecondTimer = DateTime.Now;
                }
            }
        }
    }
}

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

        /// <summary>
        /// Notify that a message has been received.
        /// </summary>
        /// <returns>The number of messages received in the last second.</returns>
        internal int NotifyMessageReceived()
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
                    var receivedInLastSecond = MessageReceivedInLastSecond;

                    MessageReceivedInLastSecond = 0;
                    _lastSecondTimer = DateTime.Now;

                    return receivedInLastSecond;
                }
            }

            return MessageReceivedInLastSecond;
        }
    }
}

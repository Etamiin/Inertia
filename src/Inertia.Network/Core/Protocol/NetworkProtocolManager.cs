using System;
using System.Linq;

namespace Inertia.Network
{
    public static class NetworkProtocolManager
    {
        internal const int DefaultBacklogQueueSize = 1000;
        internal const int DefaultMessageCountLimitBeforeSpam = 55;

        internal static ServerMessageQueuePool ServerMessagePool { get; private set; }
        public static NetworkProtocol CurrentProtocol
        {
            get
            {
                if (_currProtocol is null)
                {
                    CurrentProtocol = new DefaultNetworkProtocol(null);
                }

                return _currProtocol;
            }
            set
            {
                if (value is null)
                {
                    throw new ArgumentNullException(nameof(CurrentProtocol));
                }

                if (_currProtocol == value) return;

                _currProtocol = value;

                if (InertiaNetworkProvider.ContainsNetworkServerEntities)
                {
                    if (ServerMessagePool is null)
                    {
                        ServerMessagePool = new ServerMessageQueuePool(_currProtocol.ConnectionPerMessageQueue, null);
                    }
                    else
                    {
                        ServerMessagePool.ConnectionPerQueue = _currProtocol.ConnectionPerMessageQueue;
                    }
                }
            }
        }

        private static NetworkProtocol _currProtocol;

        static NetworkProtocolManager()
        {
            CurrentProtocol = new DefaultNetworkProtocol(null);
        }

        public static bool TryGetHandler(NetworkEntity receiver, out NetworkMessageHandler handler)
        {
            return InertiaNetworkProvider.TryGetMessageHandler(receiver, out handler);
        }
        public static void ProcessParsing(NetworkEntity receiver, DataReader reader)
        {
            var output = new MessageParsingOutput();
            
            if (!CurrentProtocol.TryParseMessage(receiver, reader, output)) return;
            if (!output.Messages.Any())
            {
                output.Dispose();
                return;
            }

            if (!TryGetHandler(receiver, out var handler))
            {
                output.Dispose();
                return;
            }

            receiver.ProcessInQueue(ExecuteOutput);

            void ExecuteOutput()
            {
                foreach (var message in output.Messages)
                {
                    handler.TryHandle(message, receiver);
                }

                output.Dispose();
            }
        }
    }
}
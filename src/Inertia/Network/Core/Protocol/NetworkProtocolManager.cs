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
                if (_currProtocol == null)
                {
                    CurrentProtocol = new DefaultNetworkProtocol();
                }

                return _currProtocol;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(CurrentProtocol));
                }

                if (_currProtocol == value) return;

                _currProtocol = value;

                if (ReflectionProvider.ContainsNetworkServerEntities)
                {
                    if (ServerMessagePool == null)
                    {
                        ServerMessagePool = new ServerMessageQueuePool(_currProtocol.ConnectionPerMessageQueue);
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
        }

        public static bool TryGetHandler(NetworkEntity receiver, out NetworkMessageHandler handler)
        {
            return ReflectionProvider.TryGetMessageHandler(receiver, out handler);
        }
        public static void ProcessParsing(NetworkProtocol protocol, NetworkEntity receiver, DataReader reader)
        {
            var output = new MessageParsingOutput();
            
            if (!protocol.TryParseMessage(receiver, reader, output)) return;
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
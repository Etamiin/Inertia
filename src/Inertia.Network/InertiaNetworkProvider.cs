using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Inertia.Network
{
    internal static class InertiaNetworkProvider
    {
        internal sealed class NetworkTypeInterceptor : TypeLoaderInterceptor
        {
            internal NetworkTypeInterceptor()
            {
            }

            protected override void Intercept(Type type)
            {
                if (!ContainsNetworkServerEntities)
                {
                    ContainsNetworkServerEntities = type.IsSubclassOf(typeof(TcpServerEntity)) || type.IsSubclassOf(typeof(WebSocketServerEntity));
                }

                if (type.IsSubclassOf(typeof(NetworkMessage)))
                {
                    if (type.IsAbstract) return;

                    var message = (NetworkMessage)Activator.CreateInstance(type);
                    if (!_messageTypes.ContainsKey(message.MessageId))
                    {
                        _messageTypes.Add(message.MessageId, type);
                    }
                }
                else if (type.IsSubclassOf(typeof(NetworkEntity)))
                {
                    var indirectEntityType = type
                        .GetInterfaces()
                        .FirstOrDefault((interfaceType) => interfaceType.GetCustomAttribute<IndirectNetworkEntityAttribute>() != null);

                    if (indirectEntityType != null)
                    {
                        _indirectNetworkEntityTypes.Add(type, indirectEntityType);
                    }
                }
                else if (typeof(IMessageHandler).IsAssignableFrom(type))
                {
                    var sMethods = type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic);
                    if (sMethods.Length == 0) return;

                    foreach (var smethod in sMethods)
                    {
                        var ps = smethod.GetParameters();
                        if (ps.Length == 2 && ps[0].ParameterType.IsSubclassOf(typeof(NetworkMessage)))
                        {
                            var isValidEntity =
                                ps[1].ParameterType.IsSubclassOf(typeof(NetworkClientEntity)) ||
                                ps[1].ParameterType.IsSubclassOf(typeof(NetworkConnectionEntity)) ||
                                ps[1].ParameterType.GetCustomAttribute<IndirectNetworkEntityAttribute>() != null;

                            if (!isValidEntity) continue;

                            var msgType = ps[0].ParameterType;
                            var entityType = ps[1].ParameterType;

                            if (!_messagesHandlerPerEntity.TryGetValue(entityType, out var handler))
                            {
                                handler = new NetworkMessageHandler();
                                _messagesHandlerPerEntity.Add(entityType, handler);
                            }

                            handler.RegisterReference(msgType, smethod);
                        }
                    }
                }
            }
        }

        internal static bool ContainsNetworkServerEntities { get; private set; }

        private readonly static Dictionary<Type, NetworkMessageHandler> _messagesHandlerPerEntity;
        private readonly static Dictionary<ushort, Type> _messageTypes;
        private readonly static Dictionary<Type, Type> _indirectNetworkEntityTypes;

        static InertiaNetworkProvider()
        {
            _messagesHandlerPerEntity = new Dictionary<Type, NetworkMessageHandler>();
            _messageTypes = new Dictionary<ushort, Type>();
            _indirectNetworkEntityTypes = new Dictionary<Type, Type>();

            ReflectionProvider.Invalidate();
        }

        internal static bool TryCreateNetworkMessage(ushort messageId, out NetworkMessage message)
        {
            if (_messageTypes.TryGetValue(messageId, out var messageType))
            {
                message = (NetworkMessage)Activator.CreateInstance(messageType);
            }
            else
            {
                message = null;
            }

            return message != null;
        }
        internal static bool TryGetMessageHandler(NetworkEntity receiver, out NetworkMessageHandler handler)
        {
            return
                _messagesHandlerPerEntity.TryGetValue(receiver.GetType(), out handler) ||
                (_indirectNetworkEntityTypes.TryGetValue(receiver.GetType(), out var indirectType) && _messagesHandlerPerEntity.TryGetValue(indirectType, out handler));
        }
    }
}

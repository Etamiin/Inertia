using System;
using System.Collections.Generic;

namespace Inertia.Network
{
    internal class NetworkMessageHandlerInvoker
    {
        private readonly Dictionary<Type, Action<NetworkMessage, NetworkEntity>> _handlerCalls;

        internal NetworkMessageHandlerInvoker()
        {
            _handlerCalls = new Dictionary<Type, Action<NetworkMessage, NetworkEntity>>();
        }

        internal void Register(Type messageType, Action<NetworkMessage, NetworkEntity> handlerCallback)
        {
            if (!_handlerCalls.TryGetValue(messageType, out _))
            {
                _handlerCalls.Add(messageType, handlerCallback);
            }
            else
            {
                throw new InvalidOperationException($"Message handler for '{messageType.Name}' is already registered for this entity.");
            }
        }
        internal void Register(NetworkMessageHandlerInvoker invoker)
        {
            foreach (var kvp in invoker._handlerCalls)
            {
                if (!_handlerCalls.TryGetValue(kvp.Key, out _))
                {
                    _handlerCalls.Add(kvp.Key, kvp.Value);
                }
                else
                {
                    throw new InvalidOperationException($"Message handler for '{kvp.Key.Name}' is already registered for this entity.");
                }
            }
        }
        internal bool TryInvokeHandleMethod(NetworkMessage message, object receiver)
        {
            if (_handlerCalls.TryGetValue(message.GetType(), out var call))
            {
                call(message, (NetworkEntity)receiver);
                return true;
            }

            return false;
        }
    }
}

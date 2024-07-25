using System;
using System.Collections.Generic;
using System.Reflection;

namespace Inertia.Network
{
    public sealed class NetworkMessageHandler
    {
        private readonly Dictionary<Type, MethodInfo> _methodHandlers;

        internal NetworkMessageHandler()
        {
            _methodHandlers = new Dictionary<Type, MethodInfo>();
        }

        internal void RegisterReference(Type messageType, MethodInfo method)
        {
            if (!_methodHandlers.ContainsKey(messageType))
            {
                _methodHandlers.Add(messageType, method);
            }
            else
            {
                _methodHandlers[messageType] = method;
            }
        }

        public bool TryHandle(NetworkMessage message, object receiver)
        {
            if (_methodHandlers.TryGetValue(message.GetType(), out MethodInfo method))
            {
                method.Invoke(null, new object[] { message, receiver });
                return true;
            }

            return false;
        }
    }
}
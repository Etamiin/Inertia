using System;
using System.Collections.Generic;
using System.Reflection;

namespace Inertia.Network
{
    public sealed class NetworkMessageHandler
    {
        private readonly Dictionary<Type, MethodInfo> _references;

        internal NetworkMessageHandler()
        {
            _references = new Dictionary<Type, MethodInfo>();
        }

        internal void RegisterReference(Type messageType, MethodInfo method)
        {
            if (!_references.ContainsKey(messageType))
            {
                _references.Add(messageType, method);
            }
            else
            {
                _references[messageType] = method;
            }
        }

        public bool TryHandle(NetworkMessage message, object receiver)
        {
            if (_references.TryGetValue(message.GetType(), out MethodInfo method))
            {
                method.Invoke(null, new object[] { message, receiver });
                return true;
            }

            return false;
        }
    }
}
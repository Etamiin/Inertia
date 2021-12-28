using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Network
{
    public sealed class NetworkMessageCaller
    {
        private readonly Dictionary<Type, MethodInfo> _references;

        internal NetworkMessageCaller()
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

        public void CallReferences(NetworkMessage message, object receiver)
        {
            if (_references.TryGetValue(message.GetType(), out MethodInfo method))
            {
                method.Invoke(null, new object[] { message, receiver });
            }
        }
    }
}

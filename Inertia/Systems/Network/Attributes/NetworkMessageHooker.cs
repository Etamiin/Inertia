using Inertia.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Network
{
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class NetworkMessageHooker : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        public Type MessageType;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageType"></param>
        public NetworkMessageHooker(Type messageType)
        {
            if (!messageType.IsSubclassOf(typeof(NetworkMessage)))
                throw new HookerInvalidTypeException(messageType);

            MessageType = messageType;
        }
    }
}

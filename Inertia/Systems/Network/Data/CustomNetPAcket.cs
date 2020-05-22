using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Network
{
    /// <summary>
    /// Custom network packet
    /// </summary>
    [Serializable]
    public abstract class CustomNetPacket : NetPacket
    {
        /// <summary>
        /// Serialize the current packet instance
        /// </summary>
        /// <param name="writer">The writer to use for serialization</param>
        internal protected abstract void OnSerialize(SimpleWriter writer);
        /// <summary>
        /// Deserialization method for the current packet instance
        /// </summary>
        /// <param name="reader">The reader to use for deserialization</param>
        internal protected abstract void OnDeserialize(SimpleReader reader);
    }
}

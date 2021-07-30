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
    public abstract class NetworkMessage
    {
        #region Public variables

        /// <summary>
        /// Get the id of the current packet
        /// </summary>
        public abstract uint Id { get; }

        #endregion

        /// <summary>
        /// Serialize the current packet instance
        /// </summary>
        /// <param name="writer">The writer to use for serialization</param>
        public virtual void OnSerialize(BasicWriter writer) { }
        /// <summary>
        /// Deserialization method for the current packet instance
        /// </summary>
        /// <param name="reader">The reader to use for deserialization</param>
        public virtual void OnDeserialize(BasicReader reader) { }
    }
}

using System;

namespace Inertia.Network
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public abstract class NetworkMessage
    {
        /// <summary>
        /// Returns the Message ID.
        /// </summary>
        public abstract uint Id { get; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="writer"></param>
        public virtual void OnSerialize(BasicWriter writer) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        public virtual void OnDeserialize(BasicReader reader) { }
    }
}

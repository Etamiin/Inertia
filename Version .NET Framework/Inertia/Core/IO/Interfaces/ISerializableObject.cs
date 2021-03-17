using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia
{
    /// <summary>
    /// 
    /// </summary>
    public interface ISerializableObject
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        void Serialize(BasicWriter writer);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        void Deserialize(BasicReader reader);
    }
}

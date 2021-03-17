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
    public interface ISerializableData
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        byte[] Serialize();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        void Deserialize(byte[] data);
    }
}

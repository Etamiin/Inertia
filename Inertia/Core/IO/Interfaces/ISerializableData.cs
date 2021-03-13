using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia
{
    public interface ISerializableData
    {
        byte[] Serialize();
        void Deserialize(byte[] data);
    }
}

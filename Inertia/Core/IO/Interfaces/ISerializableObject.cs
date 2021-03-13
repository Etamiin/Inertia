using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia
{
    public interface ISerializableObject
    {
        void Serialize(BasicWriter writer);
        void Deserialize(BasicReader reader);
    }
}

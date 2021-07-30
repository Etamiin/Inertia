using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Storage
{
    internal class InFileStorageMark
    {
        internal readonly string Name;
        internal readonly long StartPosition;
        
        internal InFileStorageMark(string name, long startPosition)
        {
            Name = name;
            StartPosition = startPosition;
        }

        public InFileStorageElement Retrieve(BasicReader reader)
        {
            reader.Position = StartPosition;
            return new InFileStorageElement(Name, reader.GetBytes(reader.GetLong()));
        }
    }
}

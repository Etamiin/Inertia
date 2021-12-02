using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.ORM
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class BlobSpecifier : Attribute
    {
        internal readonly BlobSize Size;

        public BlobSpecifier(BlobSize size)
        {
            Size = size;
        }
    }
}

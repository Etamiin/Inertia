using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Storage
{
    public class InvalidSerializableTypeException : Exception
    {
        #region Public variables

        public override string Message => GetMessage();

        #endregion

        #region Private variables

        private readonly string TypeName;

        #endregion

        #region Constructors

        public InvalidSerializableTypeException(string typeName)
        {
            TypeName = typeName;
        }

        #endregion

        private string GetMessage()
        {
            return
                "Cannot create specified InertiaSerializable Type (name: " + TypeName + ", does not exist";
        }
    }
}

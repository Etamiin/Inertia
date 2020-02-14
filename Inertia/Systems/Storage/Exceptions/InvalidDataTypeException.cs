using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Storage
{
    public class InvalidDataTypeException : Exception
    {
        #region Public variables

        public override string Message => GetMessage();

        #endregion

        #region Private variables

        private readonly object Value;

        #endregion

        #region Constructors

        public InvalidDataTypeException(object value)
        {
            Value = value;
        }

        #endregion

        private string GetMessage()
        {
            return
                "The target Type (" + Value.GetType() + ") is not serializable, please use " + nameof(ISerializableObject);
        }
    }
}

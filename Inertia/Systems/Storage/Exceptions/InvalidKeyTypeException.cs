using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Storage
{
    public class InvalidKeyTypeException : Exception
    {
        #region Public variables

        public override string Message => GetMessage();

        #endregion

        #region Private variables

        private readonly object Value;

        #endregion

        #region Constructors

        public InvalidKeyTypeException(object value)
        {
            Value = value;
        }

        #endregion

        private string GetMessage()
        {
            return
                "The target Type (" + Value.GetType() + ") is not valid for the key parameter";
        }
    }
}

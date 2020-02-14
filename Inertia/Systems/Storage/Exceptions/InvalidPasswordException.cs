using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Storage
{
    public class InvalidPasswordException : Exception
    {
        #region Public variables

        public override string Message => GetMessage();

        #endregion

        #region Constructors

        public InvalidPasswordException()
        {
        }

        #endregion

        private string GetMessage()
        {
            return
                "Can't open target storage file: password not match";
        }
    }
}

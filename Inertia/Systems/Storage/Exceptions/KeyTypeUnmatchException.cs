using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Storage
{
    public class KeyTypeUnmatchException : Exception
    {
        #region Public variables

        public override string Message => GetMessage();

        #endregion

        #region Private variables

        private readonly TypeCode Key1;
        private readonly TypeCode Key2;

        #endregion

        #region Constructors

        public KeyTypeUnmatchException(TypeCode key1, TypeCode key2)
        {
            Key1 = key1;
            Key2 = key2;
        }

        #endregion

        private string GetMessage()
        {
            return
                "Specified key Type not match from loaded file and current specified Type (CurrentType: " + Key1 + ", FileKeyType: " + Key2 + ")";
        }
    }
}

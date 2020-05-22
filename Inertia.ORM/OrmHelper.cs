using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Internal
{
    internal static class OrmHelper
    {
        #region Private variables

        private static char[] m_letters = new char[] { 
            'a', 'A', 'b', 'B', 'c', 'C', 'd', 'D', 'e', 'E', 'f', 'F', 'g',
            'G', 'h', 'H', 'i', 'I', 'j', 'J', 'k', 'K', 'l', 'L', 'm', 'M',
            'n', 'N', 'o', 'O', 'p', 'P', 'q', 'Q', 'r', 'R', 's', 'S', 't',
            'T', 'u', 'U', 'v', 'V', 'w', 'W', 'x', 'X', 'y', 'Y', 'z', 'Z',
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'
        };
        private static Random m_rand = new Random();

        #endregion

        public static string GetRandomName(int length)
        {
            string r = "@";
            for (var i = 0; i < length; i++)
                r += m_letters[m_rand.Next(0, m_letters.Length)];

            return r;
        }
        public static object InstantiateObject(Type targetType)
        {
            var constructor = targetType.GetConstructor(Type.EmptyTypes);
            if (constructor == null)
                constructor = targetType.GetConstructors()[0];
            var parameters = constructor.GetParameters();
            var objs = new object[parameters.Length];
            for (var i = 0; i < parameters.Length; i++)
                objs[i] = null;

            return constructor.Invoke(objs);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia
{
    public class InertiaApp
    {
        private static InertiaApp _instance;

        public static InertiaApp App
        {
            get
            {
                if (_instance == null) {
                    _instance = new InertiaApp();
                }
                return _instance;
            }
        }
    }
}

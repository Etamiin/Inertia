using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia
{
    public class RuntimeModule
    {
        private static RuntimeModule _instance;
        public static RuntimeModule Module
        {
            get
            {
                if (_instance == null) {
                    _instance = new RuntimeModule();
                }
                return _instance;
            }
        }

        internal RuntimeModule()
        {

        }

        public static void ExecuteAsync(ActionHandler handler)
        {
            Task.Factory.StartNew(() => handler());
        }
    }
}

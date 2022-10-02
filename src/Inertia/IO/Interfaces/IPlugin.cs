using System;
using System.Collections.Generic;
using System.Text;

namespace Inertia.IO
{
    internal interface IPlugin
    {
        public string Identifier { get; }
        public bool AutoExecute { get; }

        public void OnInitialize();
        public void OnExecute(object[] parameters);
    }
}

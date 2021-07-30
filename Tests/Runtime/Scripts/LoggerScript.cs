using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inertia;
using Inertia.Runtime;

namespace InertiaTests.Runtime
{
    public class LoggerScript : Script
    {
        private string _name;
        private int _execution;
        private int _maxExecution;

        protected override void OnAwake(ScriptArgumentsCollection args)
        {
            _name = args.GetNextArgument<string>();
            _maxExecution = args.GetArgumentAt<int>(1);
            //_maxExecution = args.GetNextArgument<int>();
        }

        protected override void OnUpdate()
        {
            Console.WriteLine($"Execution from { _name } n°{ _execution++ }");
            if (_execution == _maxExecution)
                Destroy();
        }

        protected override void OnDestroy()
        {
            Console.WriteLine($"{ _name } - End");
        }

    }
}

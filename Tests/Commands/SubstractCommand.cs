using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inertia;

namespace InertiaTests.Commands
{
    public class SubstractCommand : TextCommand
    {
        public override string Name => "substract";
        public override void Execute(TextCommandArgs args)
        {
            if (args.Count < 2)
            {
                Console.WriteLine("Invalid arguments");
                return;
            }

            Console.WriteLine($"Executing { Name } command, combined args: { args.CombineAllArguments() }");

            if (int.TryParse(args[0], out int val1) && int.TryParse(args[1], out int val2))
            {
                Console.WriteLine($"Substract: { val1 } - { val2 } = { val1 - val2 }");
            }
            else
                Console.WriteLine("Invalid values");
        }
    }
}

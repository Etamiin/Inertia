using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inertia;

namespace InertiaTests.Commands
{
    public class RandomNumberCommand : TextCommand
    {
        public override string Name => "randomnumber";
        public override void Execute(TextCommandArgs args)
        {
            var r = new Random();
            var max = 100;

            if (args.DataCount > 0)
                max = args.GetDataAt<int>(0);

            Console.WriteLine(r.Next(0, max));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inertia;

namespace NFramework_TConsole
{
    class Program
    {
        private static bool _withInput = true;

        static void Main(string[] args)
        {
            //InertiaTestsCommands();
            //InertiaTestsIO();
            //InertiaTestsQueue();
            //InertiaTestsTools();
            //InertiaTestsWeb();
            //InertiaTestNetwork();
            //InertiaTestRuntime();
            InertiaTestORM();

            Console.ReadKey();
        }

        private static void InertiaTestsCommands()
        {
            Begin("Commands");
            
            CommandHooker.TryExecuteCommandByName("randomnumber", new object[] { 200 });
            CommandHooker.TryExecuteTextCommand("substract 21 7");

            End("Commands");
        }
        private static void InertiaTestsIO()
        {
            Begin("IO");
            new InertiaTests.IO.InertiaIOTest();
            End("IO");
        }
        private static void InertiaTestsQueue()
        {
            Begin("Queue");
            new InertiaTests.Queue.InertiaTestQueue();
            End("Queue");
        }
        private static void InertiaTestsTools()
        {
            Begin("TOOLS");
            new InertiaTests.Tools.InertiaTestTools();
            End("TOOLS");
        }
        private static void InertiaTestsWeb()
        {
            Begin("Web");
            new InertiaTests.Web.InertiaWebTest();
            End("Web");
        }
        private static void InertiaTestNetwork()
        {
            Begin("NETWORK");
            new InertiaTests.Network.InertiaTestNetwork();
            End("NETWORK");
        }
        private static void InertiaTestRuntime()
        {
            Begin("RUNTIME");

            new InertiaTests.Runtime.InertiaTestRuntime();
            
            End("RUNTIME");
        }
        private static void InertiaTestORM()
        {
            Begin("ORM");

            new InertiaTests.ORM.InertiaTestORM();

            End("ORM");
        }

        private static void Begin(string name)
        {
            Console.WriteLine($"---------TESTS { name }---------");

            if (!_withInput)
                return;

            Console.WriteLine("Enter key...");
            Console.ReadKey();
        }
        private static void End(string name)
        {
            Console.WriteLine($"---------END TESTS { name }---------");
        }
    }
}

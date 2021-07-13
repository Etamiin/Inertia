using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InertiaExamples
{
    class Program
    {
        static void Main(string[] args)
        {
            //Initialize network example
            _ = new ExampleNetwork();
            _ = new ExampleLogger();
            _ = new ExampleRealtime();

            Console.Read();
        }
    }
}

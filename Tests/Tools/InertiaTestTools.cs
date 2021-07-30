using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inertia;

namespace InertiaTests.Tools
{
    public class InertiaTestTools
    {
        public InertiaTestTools()
        {
            Console.WriteLine("-- Tool Clock --");
            var clock = new Clock();

            Console.WriteLine("Wait 2seconds...");
            System.Threading.Thread.Sleep(2000);

            Console.WriteLine($"{ clock.GetElapsedMilliseconds() }ms");
            Console.WriteLine($"{ clock.GetElapsedSeconds() }s");
        }
    }
}

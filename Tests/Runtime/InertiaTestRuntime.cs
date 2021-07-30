using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inertia;
using Inertia.Runtime;

namespace InertiaTests.Runtime
{
    public class InertiaTestRuntime
    {

        public InertiaTestRuntime()
        {
            var collection = new ScriptCollection();
            var logger0 = collection.Add<LoggerScript>("logger0", 10);
            var logger1 = collection.Add<LoggerScript>("logger1", 100);
            var logger2 = collection.Add<LoggerScript>("logger2", 100);

            var clock = new Clock();
            var clock2 = new Clock();
            var c22 = false;

            new ExecuteScriptIn(.1f, (sit) => {
                var ms = clock.GetElapsedMillisecondsAndReset();
                Console.WriteLine($"From sit: { ms }ms");

                if (!c22 && clock2.GetElapsedSeconds() >= .5f)
                {
                    var ss = collection.GetScripts<LoggerScript>();
                    if (ss.Length > 0)
                    {
                        foreach (var s in ss)
                            s.Destroy();
                    }

                    c22 = true;
                }
                else if (clock2.GetElapsedSeconds() >= 1.5f)
                {
                    sit.Permanent = false;
                }
            }, true);

            System.Threading.Thread.Sleep(3000);
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inertia;

namespace InertiaTests.Queue
{
    public class InertiaTestQueue
    {
        public InertiaTestQueue()
        {
            var autoQueue = new AutoQueueExecutor();
            var manualQueue = new ManualQueueExecutor();

            for (var i = 0; i < 5; i++)
            {
                var index = i;

                autoQueue.Enqueue(new BasicAction(() => Console.WriteLine($"Action from auto: { index }")));
                manualQueue.Enqueue(new BasicAction(() => Console.WriteLine($"Action from manual: { index }")));
            }

            manualQueue.Execute();
        }
    }
}

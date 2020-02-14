using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Inertia.Scripting;

namespace Inertia
{
    public class AutoQueue : IDisposable
    {
        #region Public variables

        public bool IsDisposed { get; private set; }
        public int Count
        {
            get
            {
                return 0;
            }
        }

        #endregion

        #region Private variables

        private List<InertiaAction> Actions;
        private readonly QueueMod Mod;
        private readonly Clock Clock;

        #endregion

        #region Constructors

        public AutoQueue(QueueMod mod)
        {
            Actions = new List<InertiaAction>();
            Mod = mod;
            Clock = Clock.Create();

            DependentThread.CreateDependentTask(Execute);
        }

        #endregion

        public void Enqueue(InertiaAction handler)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(AutoQueue));

            lock (Actions)
                Actions.Add(handler);
        }
        public void EnqueueRange(params InertiaAction[] handlers)
        {
            foreach (var handler in handlers)
                Enqueue(handler);
        }
        
        private void Execute(Thread dependency)
        {
            while (dependency.IsAlive && !IsDisposed)
            {
                if (Count == 0)
                {
                    if (Mod == QueueMod.AutoDispose && Clock.GetElapsedMilliseconds() >= InertiaConfiguration.AutoQueueDisposeTime)
                    {
                        Dispose();
                        break;
                    }
                }

                lock (Actions)
                {
                    foreach (var action in Actions)
                        action();

                    Actions.Clear();
                }

                Clock.Reset();
                Thread.Sleep(InertiaConfiguration.AutoQueueSleepTime);
            }
        }
        
        public void Dispose()
        {
            IsDisposed = true;
            Clock.Dispose();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Inertia
{
    public class DependentThread : IDisposable
    {
        public static void CreateDependentTask(InertiaAction<Thread> action)
        {
            var dependency = Thread.CurrentThread;
            Task.Factory.StartNew(() => action(dependency));
        }

        #region Public variables

        public bool IsDisposed { get; private set; }

        #endregion

        #region Private variables

        private Thread Thread;

        #endregion

        #region Constructors

        public DependentThread(InertiaAction<Thread> action)
        {
            var dependency = Thread.CurrentThread;
            Thread = new Thread(() => action(dependency));
        }

        #endregion

        public void Start()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(DependentThread));

            Thread.Start();
        }
        public void Abort()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(DependentThread));

            Thread.Abort();
        }

        public void Dispose()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(DependentThread));

            IsDisposed = true;

            Thread.Abort();
            Thread = null;
        }
    }
}

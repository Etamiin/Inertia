using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia
{
    public class ManualQueue : IDisposable
    {
        #region Events

        private event InertiaAction QueueHandler = () => { };

        #endregion

        #region Public variables

        public bool IsDisposed { get; private set; }

        #endregion

        #region Constructors 

        public ManualQueue()
        {
        }

        #endregion

        public void Enqueue(InertiaAction action)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(ManualQueue));

            void handler()
            {
                action();
                QueueHandler -= handler;
            }

            QueueHandler += handler;
        }
        public void EnqueueRange(params InertiaAction[] actions)
        {
            foreach (var action in actions)
                Enqueue(action);
        }
        
        public void Execute()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(ManualQueue));

            lock (QueueHandler)
                QueueHandler();
        }
        
        public void Dispose()
        {
            IsDisposed = true;
            QueueHandler = null;
        }
    }
}

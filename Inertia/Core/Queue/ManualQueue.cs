using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia
{
    /// <summary>
    /// Queue actions and execute them manually
    /// </summary>
    public class ManualQueue : IDisposable
    {
        #region Events

        private event BasicAction QueueExecutor = () => { };

        #endregion

        #region Public variables

        /// <summary>
        /// Return true if <see cref="Dispose"/> was called
        /// </summary>
        public bool IsDisposed { get; private set; }
        /// <summary>
        /// Return the number of actions currently queued
        /// </summary>
        public int Count { get; private set; }

        #endregion

        #region Constructors 

        /// <summary>
        /// Create a new instance
        /// </summary>
        public ManualQueue()
        {
        }

        #endregion

        /// <summary>
        /// Enqueue the specified actions at the end of the queue
        /// </summary>
        /// <param name="actions">Actions to enqueue</param>
        /// <returns>Return the current instance</returns>
        public ManualQueue Enqueue(params BasicAction[] actions)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(ManualQueue));

            foreach (var action in actions)
            {
                void handler()
                {
                    try
                    {
                        action();
                    }
                    catch (Exception ex) { BaseLogger.DefaultLogger.Log(ex); }

                    QueueExecutor -= handler;
                    Count--;
                }

                QueueExecutor += handler;
                Count++;
            }

            return this;
        }

        /// <summary>
        /// Execute all the actions queued and remove them from the queue
        /// </summary>
        public void Execute()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(ManualQueue));

            lock (QueueExecutor)
                QueueExecutor();
        }
        
        /// <summary>
        /// Dispose the current instance
        /// </summary>
        public void Dispose()
        {
            IsDisposed = true;
            QueueExecutor = null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia
{
    /// <summary>
    ///
    /// </summary>
    public class ManualQueueExecutor : IDisposable
    {
        private event BasicAction QueueExecutor = () => { };

        /// <summary>
        /// Return true if the current instance is disposed
        /// </summary>
        public bool IsDisposed { get; private set; }
        /// <summary>
        /// Return the number of actions currently queued
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Initialize a new instance of the class <see cref="ManualQueueExecutor"/>
        /// </summary>
        public ManualQueueExecutor()
        {
        }

        /// <summary>
        /// Enqueue the specified actions at the end of the queue
        /// </summary>
        /// <param name="actions">Actions to enqueue</param>
        /// <returns>Return the current instance</returns>
        public ManualQueueExecutor Enqueue(params BasicAction[] actions)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(ManualQueueExecutor));

            foreach (var action in actions)
            {
                void handler()
                {
                    try
                    {
                        action();
                    }
                    catch (Exception ex) { this.GetLogger().Log(ex); }

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
                throw new ObjectDisposedException(nameof(ManualQueueExecutor));

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

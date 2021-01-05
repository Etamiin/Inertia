using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Inertia
{
    /// <summary>
    /// Queue actions and execute them automatically
    /// </summary>
    public class AutoQueueExecutor : IDisposable
    {
        #region Public variables

        /// <summary>
        /// Return true if <see cref="Dispose"/> was called
        /// </summary>
        public bool IsDisposed { get; private set; }
        /// <summary>
        /// Return the number of actions currently queued
        /// </summary>
        public int Count
        {
            get
            {
                lock (m_queue)
                    return m_queue.Count;
            }
        }

        #endregion

        #region Private variables

        private List<BasicAction> m_queue;

        #endregion

        #region Constructors

        /// <summary>
        /// Create a new instance
        /// </summary>
        public AutoQueueExecutor()
        {
            m_queue = new List<BasicAction>();;

            Task.Factory.StartNew(() => { 
                while (!IsDisposed)
                {
                    Execute();
                }
            });
        }

        #endregion

        /// <summary>
        /// Enqueue the specified actions at the end of the queue
        /// </summary>
        /// <param name="handlers">Actions to enqueue</param>
        public void Enqueue(params BasicAction[] handlers)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(AutoQueueExecutor));

            foreach (var handler in handlers)
                m_queue.Add(handler);
        }
        
        /// <summary>
        /// Dispose the current instance
        /// </summary>
        public void Dispose()
        {
            IsDisposed = true;
            m_queue.Clear();
            m_queue = null;
        }

        private void Execute()
        {
            if (Count == 0)
            {
                Thread.Sleep(50);
                return;
            }

            lock (m_queue)
            {
                for (var i = 0; i < m_queue.Count; i++)
                {
                    var action = m_queue[i];
                    try
                    {
                        action();
                    }
                    catch (Exception ex) { this.GetLogger().Log(ex); }
                }

                m_queue.Clear();
            }

            Thread.Sleep(1);
        }
    }
}

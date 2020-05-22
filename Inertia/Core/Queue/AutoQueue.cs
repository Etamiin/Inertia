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
    public class AutoQueue : IDisposable
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

        private List<SimpleAction> m_queue;
        private Clock m_clock;

        #endregion

        #region Constructors

        /// <summary>
        /// Create a new instance
        /// </summary>
        public AutoQueue()
        {
            m_queue = new List<SimpleAction>();;
            m_clock = new Clock();

            DependentThread.ExecuteTaskWhileDependencyIsAlive(Execute, additionalConditions: () => !IsDisposed);
        }

        #endregion

        /// <summary>
        /// Enqueue the specified actions at the end of the queue
        /// </summary>
        /// <param name="handlers">Actions to enqueue</param>
        public void Enqueue(params SimpleAction[] handlers)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(AutoQueue));

            lock (m_queue)
            {
                foreach (var handler in handlers)
                    m_queue.Add(handler);
            }
        }
        
        /// <summary>
        /// Dispose the current instance
        /// </summary>
        public void Dispose()
        {
            IsDisposed = true;
            m_queue.Clear();
            m_clock.Dispose();
            m_queue = null;
            m_clock = null;
        }

        private void Execute()
        {
            if (Count == 0)
                Thread.Sleep(10);

            lock (m_queue)
            {
                foreach (var action in m_queue)
                    action();

                m_queue.Clear();
            }

            m_clock.Reset();
            Thread.Sleep(1);

        }
    }
}

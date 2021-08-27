using System;
using System.Collections.Generic;

namespace Inertia.Runtime
{
    /// <summary>
    ///
    /// </summary>
    public sealed class ScriptCollection : IDisposable
    {
        /// <summary>
        /// Returns the number of <see cref="Script"/> running in the collection.
        /// </summary>
        public int Count
        {
            get
            {
                return m_scripts.Count;
            }
        }
        /// <summary>
        /// Returns true if the currnet instance is disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }

        private List<Script> m_scripts;

        /// <summary>
        /// Initialize a new instance of the class <see cref="ScriptCollection"/>
        /// </summary>
        public ScriptCollection()
        {
            m_scripts = new List<Script>();
        }

        /// <summary>
        /// Create a new instance of <typeparamref name="T"/> with the specified arguments.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> of the target <see cref="Script"/></typeparam>
        /// <param name="args"></param>
        /// <returns>Returns the created instance of <typeparamref name="T"/></returns>
        public T Add<T>(params object[] args) where T : Script
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(ScriptCollection));

            var s = (Script)typeof(T).GetConstructor(Type.EmptyTypes).Invoke(new object[] { });

            s.InCollection = this;
            s.Awake(args);

            lock (m_scripts)
                m_scripts.Add(s);

            return (T)s;
        }

        /// <summary>
        /// Remove the first occurence of type <typeparamref name="T"/> in the collection.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> of <see cref="Script"/> to remove</typeparam>
        public void Remove<T>() where T : Script
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(ScriptCollection));

            lock (m_scripts)
            {
                try
                {
                    var script = m_scripts.Find((s) => s.GetType() == typeof(T) && !s.IsDisposed);
                    if (script != null)
                        script.Dispose();
                }
                catch { }
            }
        }
        /// <summary>
        /// Remove all <see cref="Script"/> of type <typeparamref name="T"/> running in the collection.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> of <see cref="Script"/> to remove</typeparam>
        public void RemoveAll<T>() where T : Script
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(ScriptCollection));

            lock (m_scripts)
            {
                try
                {
                    var scripts = m_scripts.FindAll((s) => s.GetType() == typeof(T) && !s.IsDisposed);
                    foreach (var script in scripts)
                    {
                        if (script != null)
                            script.Dispose();
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// Get the first occurence of the specified <see cref="Script"/> of type <typeparamref name="T"/> or null.
        /// </summary>
        /// <typeparam name="T">The target <see cref="Script"/> to get</typeparam>
        /// <returns></returns>
        public T GetScript<T>() where T : Script
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(ScriptCollection));

            lock (m_scripts)
            {
                return m_scripts.Find((s) => s.GetType() == typeof(T)) as T;
            }
        }
        /// <summary>
        /// Get all scripts of type <typeparamref name="T"/> in the collection.
        /// </summary>
        /// <typeparam name="T">The target <see cref="Script"/> to get</typeparam>
        /// <returns></returns>
        public T[] GetScripts<T>() where T : Script
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(ScriptCollection));

            var result = new List<T>();

            lock (m_scripts)
            {
                foreach (var s in m_scripts)
                {
                    if (s.GetType() == typeof(T))
                        result.Add((T)s);
                }
            }

            return result.ToArray();
        }

        internal void FinalizeRemove(Script script)
        {
            m_scripts.Remove(script);
            script.InCollection = null;
        }

        /// <summary>
        ///
        /// </summary>
        public void Dispose()
        {
            if (IsDisposed)
                return;

            Script[] scripts;
            lock (m_scripts)
                scripts = m_scripts.ToArray();

            foreach (var script in m_scripts)
                script.Dispose();

            m_scripts.Clear();

            IsDisposed = true;
        }
    }
}

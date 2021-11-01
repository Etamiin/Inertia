using System;
using System.Collections.Generic;

namespace Inertia.Runtime
{
<<<<<<< HEAD
    /// <summary>
    ///
    /// </summary>
    public sealed class ScriptCollection : IDisposable
    {
=======
    public sealed class ScriptCollection : IDisposable
    {
        public bool IsDisposed { get; private set; }
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
        /// <summary>
        /// Returns the number of <see cref="Script"/> running in the collection.
        /// </summary>
        public int Count
        {
            get
            {
                return _scripts.Count;
            }
        }
<<<<<<< HEAD
        /// <summary>
        /// Returns true if the currnet instance is disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }
=======
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40

        private readonly List<Script> _scripts;

        /// <summary>
        /// Initialize a new instance of the class <see cref="ScriptCollection"/>
        /// </summary>
        public ScriptCollection()
        {
            _scripts = new List<Script>();
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
            {
                throw new ObjectDisposedException(nameof(ScriptCollection));
            }

            var s = (Script)typeof(T).GetConstructor(Type.EmptyTypes).Invoke(new object[] { });

            s.Parent = this;
            s.Awake(args);

            lock (_scripts)
            {
                _scripts.Add(s);
            }

            return (T)s;
        }

        /// <summary>
        /// Remove the first occurence of type <typeparamref name="T"/> in the collection.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> of <see cref="Script"/> to remove</typeparam>
        public void Remove<T>() where T : Script
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(ScriptCollection));
            }

            lock (_scripts)
            {
                var script = _scripts.Find((s) => s.GetType() == typeof(T) && !s.IsDisposed);
                if (script != null)
                {
                    script.Dispose();
                }
            }
        }
        /// <summary>
        /// Remove all <see cref="Script"/> of type <typeparamref name="T"/> running in the collection.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> of <see cref="Script"/> to remove</typeparam>
        public void RemoveAll<T>() where T : Script
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(ScriptCollection));
            }

            lock (_scripts)
            {
                var scripts = _scripts.FindAll((s) => s.GetType() == typeof(T) && !s.IsDisposed);
                foreach (var script in scripts)
                {
                    if (script != null)
                    {
                        script.Dispose();
                    }
                }
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
            {
                throw new ObjectDisposedException(nameof(ScriptCollection));
            }

            lock (_scripts)
            {
                return _scripts.Find((s) => s.GetType() == typeof(T)) as T;
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
            {
                throw new ObjectDisposedException(nameof(ScriptCollection));
            }

            var result = new List<T>();

            lock (_scripts)
            {
                foreach (var s in _scripts)
                {
                    if (s.GetType() == typeof(T))
                    {
                        result.Add((T)s);
                    }
                }
            }

            return result.ToArray();
        }

        internal void FinalizeRemove(Script script)
        {
            _scripts.Remove(script);
            script.Parent = null;
        }

<<<<<<< HEAD
        /// <summary>
        ///
        /// </summary>
=======
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
        public void Dispose()
        {
            if (!IsDisposed)
            {
                lock (_scripts)
                {
                    foreach (var script in _scripts)
                    {
                        script.Dispose();
                    }

                    _scripts.Clear();
                }

                IsDisposed = true;
            }
        }
    }
}

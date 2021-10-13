using System;

namespace Inertia.Runtime
{
    /// <summary>
    ///
    /// </summary>
    public sealed class ScriptArgumentsCollection : IDisposable
    {
        /// <summary>
        /// Returns true if the current instance is disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }
        /// <summary>
        /// Returns the number of arguments in the collection.
        /// </summary>
        public int Count
        {
            get
            {
                return _arguments.Length;
            }
        }
        /// <summary>
        /// Returns the next argument position in the collection.
        /// </summary>
        public int Position { get; set; }

        private object[] _arguments;

        internal ScriptArgumentsCollection(object[] args)
        {
            _arguments = args;
        }

        /// <summary>
        /// Returns the argument as object at the specified index in the collection.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public object this[int index]
        {
            get
            {
                return _arguments[index];
            }
        }

        /// <summary>
        /// Returns the argument as <typeparamref name="T"/> at the specified index in the collection.
        /// </summary>
        /// <typeparam name="T">Specified <see cref="Type"/> to cast</typeparam>
        /// <param name="index"></param>
        /// <returns></returns>
        public T GetArgumentAt<T>(int index)
        {
            if (index >= 0 && index < Count)
            {
                return (T)this[index];
            }
            else
            {
                throw new ArgumentNullException();
            }
        }
        /// <summary>
        /// Returns the next argument based on <see cref="Position"/> field casted to <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Target <see cref="Type"/> of the argument</typeparam>
        /// <returns></returns>
        public T GetNextArgument<T>()
        {
            if (Position >= 0 && Position < Count)
            {
                return (T)this[Position++];
            }
            else
            {
                throw new ArgumentNullException();
            }
        }

        /// <summary>
        ///
        /// </summary>
        public void Dispose()
        {
            if (!IsDisposed)
            {
                _arguments = null;
                IsDisposed = true;
            }
        }
    }
}

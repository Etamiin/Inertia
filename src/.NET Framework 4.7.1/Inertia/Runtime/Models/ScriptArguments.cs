using System;

namespace Inertia.Runtime
{
    public sealed class ScriptArguments : IDisposable
    {
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

        private object[] _arguments;

        internal ScriptArguments(object[] args)
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

        public T GetAt<T>(int index)
        {
            if (index < 0 || index >= Count)
            {
                throw new Exception("Index out of range");
            }

            var argument = this[index];
            if (typeof(T) == argument.GetType())
            {
                return (T)argument;
            }

            throw new Exception("Types mismatch");
        }

        public void Dispose()
        {
            _arguments = null;
        }
    }
}

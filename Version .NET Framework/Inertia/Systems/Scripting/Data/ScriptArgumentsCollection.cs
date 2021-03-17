using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Realtime
{
    /// <summary>
    /// Represent the arguments collection associated to a script in the initialization
    /// </summary>
    public class ScriptArgumentsCollection : IDisposable
    {
        #region Public variables

        /// <summary>
        /// Return true if the current instance is disposed
        /// </summary>
        public bool IsDisposed { get; private set; }
        /// <summary>
        /// Return the number of arguments in the collection
        /// </summary>
        public int Count
        {
            get
            {
                return m_arguments.Length;
            }
        }
        /// <summary>
        /// Return the next argument position in the collection
        /// </summary>
        public int Position;

        #endregion

        #region Private variables

        private object[] m_arguments;

        #endregion

        #region Constructors

        internal ScriptArgumentsCollection(object[] args)
        {
            m_arguments = args;
        }

        #endregion

        /// <summary>
        /// Return the argument as object at the specified index in the collection
        /// </summary>
        /// <param name="index">Target index</param>
        /// <returns>Argument as object</returns>
        public object this[int index]
        {
            get
            {
                return m_arguments[index];
            }
        }

        /// <summary>
        /// Return the argument as <typeparamref name="T"/> at the specified index in the collection
        /// </summary>
        /// <typeparam name="T">Specified <see cref="Type"/> to cast</typeparam>
        /// <param name="index">Target index</param>
        /// <returns>Argument as <typeparamref name="T"/></returns>
        public T GetArgumentAt<T>(int index)
        {
            if (index < 0 || index >= Count)
                throw new IndexOutOfRangeException();

            return (T)this[index];
        }
        /// <summary>
        /// Return the next argument based on <see cref="Position"/> field casted to <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">Target <see cref="Type"/> of the argument</typeparam>
        /// <returns>Argument as <typeparamref name="T"/></returns>
        public T GetNextArgument<T>()
        {
            if (Position < 0 || Position >= Count)
                throw new IndexOutOfRangeException();

            return (T)this[Position++];
        }

        /// <summary>
        /// Dispose the current instance
        /// </summary>
        public void Dispose()
        {
            if (IsDisposed)
                return;

            IsDisposed = true;
            m_arguments = null;
        }
    }
}

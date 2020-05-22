using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia
{
    /// <summary>
    /// Represent the class manager for the <see cref="TextCommand"/>'s arguments
    /// </summary>
    public class TextCommandArgs : IDisposable
    {
        #region Public variables

        /// <summary>
        /// Return the number of string arguments in the command
        /// </summary>
        public int Count
        {
            get
            {
                return m_arguments.Length;
            }
        }
        /// <summary>
        /// Return the number of data objects arguments in the command
        /// </summary>
        public int DataCount
        {
            get
            {
                return m_dataArguments.Length;
            }
        }
        /// <summary>
        /// Return the position of the last selected string argument
        /// </summary>
        public int Position { get; set; }

        #endregion

        #region Private variables

        private string[] m_arguments;
        private object[] m_dataArguments;

        #endregion

        #region Constructors

        internal TextCommandArgs(string[] args, object[] dataCollection)
        {
            var arguments = new List<string>();
            var inSentence = false;
            var sentence = string.Empty;

            for (var i = 0; i < args.Length; i++)
            {
                if (!inSentence && string.IsNullOrEmpty(args[i]))
                    continue;

                if (args[i].StartsWith('"'.ToString()))
                    inSentence = true;

                if (args[i].EndsWith('"'.ToString()))
                {
                    sentence += args[i];
                    arguments.Add(sentence.Substring(1, sentence.Length - 2));

                    sentence = string.Empty;
                    inSentence = false;
                }
                else {
                    if (inSentence)
                        sentence += args[i] + " ";
                    else
                        arguments.Add(args[i]);
                }
            }

            m_arguments = arguments.ToArray();
            m_dataArguments = dataCollection ?? (new object[] { });
        }

        #endregion

        /// <summary>
        /// Return the string argument at the specified index
        /// </summary>
        /// <param name="index">The index to find the argument</param>
        /// <returns>Return the string argument</returns>
        public string this[int index]
        {
            get
            {
                return m_arguments[index];
            }
        }

        /// <summary>
        /// Return the data object at the specified argument
        /// </summary>
        /// <param name="index">The index to find the argument</param>
        /// <returns>Return the data object casted to <see cref="Object"/></returns>
        public object GetDataAt(int index)
        {
            if (index < 0 || index >= m_dataArguments.Length)
                return null;

            return m_dataArguments[index];
        }
        /// <summary>
        /// Return the data object at the specified argument
        /// </summary>
        /// <typeparam name="T">The target data object type</typeparam>
        /// <param name="index">The index to find the argument</param>
        /// <returns>Return the data object casted to <typeparamref name="T"/></returns>
        public T GetDataAt<T>(int index)
        {
            var data = GetDataAt(index);
            if (data != null)
                return (T)data;

            return default;
        }

        /// <summary>
        /// Get the next string argument based on the <see cref="Position"/>
        /// </summary>
        /// <param name="argument">The string argument result</param>
        /// <returns>Return false if no string argument is available at the target position</returns>
        public bool GetNextArgument(out string argument)
        {
            if (Position < 0 || Position >= Count) {
                argument = string.Empty;
                return false;
            }

            argument = this[Position++];
            return true;
        }
        /// <summary>
        /// Get all the string arguments in the command
        /// </summary>
        /// <returns>Return an array of string representing the string arguments</returns>
        public string[] GetCollection()
        {
            return m_arguments;
        }
        /// <summary>
        /// Get all the data objects arguments in the command
        /// </summary>
        /// <returns>Return an array of object representing the data objects arguments</returns>
        public object[] GetDataCollection()
        {
            return m_dataArguments;
        }

        /// <summary>
        /// Dispose the current instance
        /// </summary>
        public void Dispose()
        {
            m_arguments = null;
            m_dataArguments = null;
        }
    }
}

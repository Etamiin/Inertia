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
        /// Get the name of the executed <see cref="TextCommand"/>
        /// </summary>
        public string Name { get; private set; }
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

        private string m_line;
        private string[] m_arguments;
        private object[] m_dataArguments;
        
        #endregion

        #region Constructors

        internal TextCommandArgs(string name, string[] args, object[] dataCollection)
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

            Name = name;
            m_arguments = arguments.ToArray();
            m_dataArguments = dataCollection ?? (new object[] { });
            m_line = name + " ";
            for (var i = 0; i < m_arguments.Length; i++)
            {
                m_line += m_arguments[i];
                if (i < m_arguments.Length - 1)
                    m_line += " ";
            }
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
        /// Combine all arguments to a string starting at the specified index
        /// </summary>
        /// <param name="startIndex">Index where to start</param>
        /// <returns></returns>
        public string CombineArguments(int startIndex)
        {
            return CombineArguments(startIndex, Count - startIndex);
        }
        /// <summary>
        /// Combine all arguments to a string starting at the specified index
        /// </summary>
        /// <param name="startIndex">Index where to start</param>
        /// <param name="length">Number of arguments to include</param>
        /// <returns></returns>
        public string CombineArguments(int startIndex, int length)
        {
            var combined = string.Empty;
            for (var i = 0; i < length; i++) {
                if (i > 0)
                    combined += " ";

                var arg = this[startIndex + i];
                if (arg.Contains(" "))
                    combined += '"' + arg + '"';
                else
                    combined += arg;
            }

            return combined;
        }
        /// <summary>
        /// Combine all arguments to a string from the start to the end
        /// </summary>
        /// <returns></returns>
        public string CombineAllArguments()
        {
            return CombineArguments(0, Count);
        }

        /// <summary>
        /// Get the base command line
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return m_line;
        }

        /// <summary>
        /// Dispose the current instance
        /// </summary>
        public void Dispose()
        {
            Name = null;
            m_arguments = null;
            m_dataArguments = null;
            m_line = null;
        }
    }
}

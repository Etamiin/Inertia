using System;
using System.Collections.Generic;

namespace Inertia
{
    /// <summary>
    /// Provides methods for accessing arguments when running a <see cref="TextCommand"/>
    /// </summary>
    public class TextCommandArgs : IDisposable
    {
        /// <summary>
        /// Returns the name of the command executed.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Returns the number of arguments of the command executed.
        /// </summary>
        public int Count
        {
            get
            {
                return m_arguments.Length;
            }
        }
        /// <summary>
        /// Returns the number of data objects of the command executed.
        /// </summary>
        public int DataCount
        {
            get
            {
                return m_dataArguments.Length;
            }
        }

        private bool m_disposed;
        private string m_line;
        private string[] m_arguments;
        private object[] m_dataArguments;
        private int m_position;

        internal TextCommandArgs(string line, string name, string[] args, object[] dataCollection)
        {
            var arguments = new List<string>();
            var inSentence = false;
            var sentence = string.Empty;

            for (var i = 0; i < args.Length; i++)
            {
                if (!inSentence && string.IsNullOrEmpty(args[i]))
                    continue;

                if (args[i].StartsWith($"{ '"' }".ToString()))
                    inSentence = true;

                if (args[i].EndsWith($"{ '"' }"))
                {
                    sentence += args[i];
                    arguments.Add(sentence.Substring(1, sentence.Length - 2));

                    sentence = string.Empty;
                    inSentence = false;
                }
                else
                {
                    if (inSentence)
                        sentence += args[i] + " ";
                    else
                        arguments.Add(args[i]);
                }
            }

            Name = name;
            m_arguments = arguments.ToArray();
            m_dataArguments = dataCollection ?? (new object[] { });
            m_line = line;
        }

        /// <summary>
        /// Returns the argument of the command executed at the specified index.
        /// </summary>
        /// <param name="index">Target index</param>
        /// <returns>The string argument</returns>
        public string this[int index]
        {
            get
            {
                return m_arguments[index];
            }
        }

        /// <summary>
        /// Returns the data argument of Type <typeparamref name="T"/> at the specified index
        /// </summary>
        /// <typeparam name="T">Target data object type</typeparam>
        /// <param name="index">Target index</param>
        /// <returns>The data object as <typeparamref name="T"/></returns>
        public T GetDataAt<T>(int index)
        {
            if (index < 0 || index >= m_dataArguments.Length)
                return default(T);

            var data = m_dataArguments[index];
            return data != null ? (T)data : default(T);
        }

        /// <summary>
        /// Returns true if an argument is available in the queue otherwise false.
        /// </summary>
        /// <param name="argument">The argument result</param>
        /// <returns></returns>
        public bool GetNextArgument(out string argument)
        {
            if (m_position < 0 || m_position >= Count)
            {
                argument = string.Empty;
                return false;
            }

            argument = this[m_position++];
            return true;
        }

        /// <summary>
        /// Combines all arguments of the executed command from the specified index.
        /// </summary>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public string CombineArguments(int startIndex)
        {
            return CombineArguments(startIndex, Count - startIndex);
        }
        /// <summary>
        /// Combines all arguments of the executed command from the specified index to the specified length.
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public string CombineArguments(int startIndex, int length)
        {
            var combined = string.Empty;
            for (var i = startIndex; i < length; i++)
            {
                if (i >= m_arguments.Length)
                    throw new IndexOutOfRangeException();

                if (i > startIndex) combined += " ";
                combined += m_arguments[i];
            }

            return combined;
        }
        /// <summary>
        /// Combines all arguments of the executed command.
        /// </summary>
        /// <returns></returns>
        public string CombineAllArguments()
        {
            return CombineArguments(0, Count);
        }

        /// <summary>
        /// Returns the command line executed.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return m_line;
        }

        /// <summary>
        ///
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (m_disposed)
                return;

            m_disposed = true;
        }
    }
}

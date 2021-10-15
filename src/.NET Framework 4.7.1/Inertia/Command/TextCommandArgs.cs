using System;
using System.Collections.Generic;
using System.Text;

namespace Inertia
{
    /// <summary>
    /// Provides methods for accessing arguments when running a <see cref="TextCommand"/>
    /// </summary>
    public sealed class TextCommandArgs : IDisposable
    {
        /// <summary>
        /// Returns the number of arguments of the command executed.
        /// </summary>
        public int Count
        {
            get
            {
                return _arguments.Length;
            }
        }
        /// <summary>
        /// Returns the number of data objects of the command executed.
        /// </summary>
        public int DataCount
        {
            get
            {
                return _dataArguments.Length;
            }
        }

        private readonly string[] _arguments;
        private readonly object[] _dataArguments;
        private readonly string _line;

        private bool _disposed;
        private int _position;

        internal TextCommandArgs(string line, string[] args, object[] dataCollection)
        {
            var arguments = new List<string>();
            var inSentence = false;
            var sentence = new StringBuilder();

            for (var i = 0; i < args.Length; i++)
            {
                if (!inSentence && string.IsNullOrEmpty(args[i]))
                {
                    continue;
                }

                if (args[i].StartsWith($"{ '"' }"))
                {
                    inSentence = true;
                }

                if (args[i].EndsWith($"{ '"' }"))
                {
                    sentence.Append(args[i]);
                    arguments.Add(sentence.ToString().Substring(1, sentence.Length - 2));

                    sentence = new StringBuilder();
                    inSentence = false;
                }
                else
                {
                    if (inSentence)
                    {
                        sentence.Append(args[i] + " ");
                    }
                    else
                    {
                        arguments.Add(args[i]);
                    }
                }
            }

            _arguments = arguments.ToArray();
            _dataArguments = dataCollection ?? (new object[] { });
            _line = line;
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
                return _arguments[index];
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
            if (index >= 0 && index < _dataArguments.Length)
            {
                var data = _dataArguments[index];
                return data != null ? (T)data : default(T);
            }

            return default(T);
        }

        /// <summary>
        /// Returns true if an argument is available in the queue otherwise false.
        /// </summary>
        /// <param name="arg">The argument result</param>
        /// <returns></returns>
        public bool TryGetNextArgument(out string arg)
        {
            if (_position >= 0 && _position < Count)
            {
                arg = this[_position++];
                return true;
            }

            arg = string.Empty;
            return false;
        }

        /// <summary>
        /// Combines all arguments of the executed command from the specified index.
        /// </summary>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public string CombineArguments(int startIndex)
        {
            return CombineArguments(startIndex, Count);
        }
        /// <summary>
        /// Combines all arguments of the executed command from the specified index to the specified length.
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public string CombineArguments(int startIndex, int length)
        {
            var combined = new StringBuilder();
            for (var i = startIndex; i < length; i++)
            {
                if (i >= _arguments.Length)
                {
                    throw new ArgumentNullException();
                }

                if (i > startIndex)
                {
                    combined.Append(" ");
                }

                combined.Append(_arguments[i]);
            }

            return combined.ToString();
        }
        /// <summary>
        /// Combines all arguments of the executed command.
        /// </summary>
        /// <returns></returns>
        public string CombineAllArguments()
        {
            return CombineArguments(0, Count);
        }

        public object[] GetAllArguments()
        {
            return GetAllArguments(0);
        }
        public object[] GetAllArguments(int startIndex)
        {
            var args = new object[Count - startIndex];
            if (args.Length > 0)
            {
                for (var i = 0; i < args.Length; i++)
                {
                    args[i] = _arguments[i + startIndex];
                }
            }

            return args;
        }

        /// <summary>
        /// Returns the command line executed.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _line;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
            }
        }
    }
}

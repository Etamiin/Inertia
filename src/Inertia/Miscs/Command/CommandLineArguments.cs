using System;

namespace Inertia
{
    public sealed class CommandLineArguments
    {
        public int Count => _arguments.Length;

        private readonly string[] _arguments;

        internal CommandLineArguments(string[] arguments)
        {
            _arguments = arguments;
        }

        public string this[int index]
        {
            get
            {
                return _arguments[index];
            }
        }
    }
}

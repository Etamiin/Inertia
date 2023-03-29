using System;

namespace Inertia
{
    public sealed class CommandArguments
    {
        public int Count => _arguments.Length;
        public object? State => _state;

        private readonly string[] _arguments;
        private readonly object? _state;

        internal CommandArguments(string[] arguments, object? state)
        {
            _arguments = arguments;
            _state = state;
        }

        public string this[int index]
        {
            get
            {
                return _arguments[index];
            }
        }

        public T GetStateAs<T>()
        {
            if (!(_state is T))
            {
                throw new ArgumentNullException(typeof(T).Name);
            }

            return (T)_state;
        }
        public bool TryGetStateAs<T>(out T state)
        {
            if (_state is T s)
            {
                state = s;
                return true;
            }

            state = default;
            return false;
        }
    }
}

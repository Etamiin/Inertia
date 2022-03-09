using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Inertia
{
    public sealed class BasicCommandArguments
    {
        public int Count => _arguments.Length;
        public int DataCount => _data.Length;

        private readonly string[] _arguments;
        private readonly object[] _data;

        internal BasicCommandArguments(string[] arguments, object[] dataCollection)
        {
            _arguments = arguments;
            _data = dataCollection;
        }

        public string this[int index]
        {
            get
            {
                return _arguments[index];
            }
        }

        public object GetDataObject(int index)
        {
            return _data[index];
        }
        public bool TryGetDataObject<T>(int index, out T data)
        {
            var obj = GetDataObject(index);
            if (obj is T)
            {
                data = (T)obj;
                return true;
            }

            data = default(T);
            return false;
        }

        public void ForEach(BasicAction<string> onArgument)
        {
            foreach (var s in _arguments)
            {
                onArgument(s);
            }
        }
    }
}

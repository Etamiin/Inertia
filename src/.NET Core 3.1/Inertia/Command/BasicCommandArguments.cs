using System;
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
        public T GetDataObject<T>(int index)
        {
            return (T)GetDataObject(index);
        }
    }
}

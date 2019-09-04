using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia
{
    public class CommandArguments : IReaderModel
    {
        public int ArgumentsLength => _args.Length;
        public int CurrentPosition => _currentIndex + 2;

        private int _currentIndex = -1;
        private bool _isValidIndex => (++_currentIndex < ArgumentsLength);
        private string[] _args;

        public CommandArguments(string[] args)
        {
            this._args = args;
            if (this._args == null)
                this._args = new string[] { };
        }

        public void GoBack()
        {
            if (_currentIndex == -1)
                return;

            _currentIndex--;
        }

        public string GetSentence(int length)
        {
            string sentence = string.Empty;
            var i = 0;
            while (i < length) {
                if (_isValidIndex)
                    sentence += _args[_currentIndex];
                i++;
            }

            return sentence;
        }
        public string GetSentence()
        {
            string sentence = string.Empty;
            while (_isValidIndex) {
                sentence += _args[_currentIndex];
            }

            return sentence;
        }

        public bool GetBool()
        {
            return _isValidIndex && bool.TryParse(_args[_currentIndex], out bool value) ? value : default(bool);
        }
        public string GetString()
        {
            return _isValidIndex ? _args[_currentIndex] : default(string);
        }
        public string GetString(Encoding encoder)
        {
            return GetString();
        }
        public float GetFloat()
        {
            return _isValidIndex && float.TryParse(_args[_currentIndex], out float value) ? value : default(float);
        }
        public decimal GetDecimal()
        {
            return _isValidIndex && decimal.TryParse(_args[_currentIndex], out decimal value) ? value : default(decimal);
        }
        public double GetDouble()
        {
            return _isValidIndex && double.TryParse(_args[_currentIndex], out double value) ? value : default(double);
        }
        public byte GetByte()
        {
            return _isValidIndex && byte.TryParse(_args[_currentIndex], out byte value) ? value : default(byte);
        }
        public sbyte GetSByte()
        {
            return _isValidIndex && sbyte.TryParse(_args[_currentIndex], out sbyte value) ? value : default(sbyte);
        }
        public char GetChar()
        {
            return _isValidIndex && char.TryParse(_args[_currentIndex], out char value) ? value : default(char);
        }
        public short GetShort()
        {
            return _isValidIndex && short.TryParse(_args[_currentIndex], out short value) ? value : default(short);
        }
        public ushort GetUShort()
        {
            return _isValidIndex && ushort.TryParse(_args[_currentIndex], out ushort value) ? value : default(ushort);
        }
        public int GetInt()
        {
            return _isValidIndex && int.TryParse(_args[_currentIndex], out int value) ? value : default(int);
        }
        public uint GetUInt()
        {
            return _isValidIndex && uint.TryParse(_args[_currentIndex], out uint value) ? value : default(uint);
        }
        public long GetLong()
        {
            return _isValidIndex && long.TryParse(_args[_currentIndex], out long value) ? value : default(long);
        }
        public ulong GetULong()
        {
            return _isValidIndex && ulong.TryParse(_args[_currentIndex], out ulong value) ? value : default(ulong);
        }
        public byte[] GetBytes()
        {
            var value = GetString();
            return value != default(string) ? Encoding.UTF8.GetBytes(value) : default(byte[]);
        }
        public object GetValue(Type valueType)
        {
            var value = (object)null;

            if (valueType == typeof(float))
                value = GetFloat();
            else if (valueType == typeof(decimal))
                value = GetDecimal();
            else if (valueType == typeof(double))
                value = GetDouble();
            else if (valueType == typeof(short))
                value = GetShort();
            else if (valueType == typeof(ushort))
                value = GetUShort();
            else if (valueType == typeof(int))
                value = GetInt();
            else if (valueType == typeof(uint))
                value = GetUInt();
            else if (valueType == typeof(string))
                value = GetString();
            else if (valueType == typeof(bool))
                value = GetBool();
            else if (valueType == typeof(byte))
                value = GetByte();
            else if (valueType == typeof(sbyte))
                value = GetSByte();
            else if (valueType == typeof(long))
                value = GetLong();
            else if (valueType == typeof(ulong))
                value = GetULong();
            else if (valueType == typeof(byte[]))
                value = GetBytes();

            return value;
        }
        public IList GetValues(params Type[] valuesType)
        {
            var values = new object[valuesType.Length];

            var i = 0;
            foreach (var type in valuesType)
                values[i++] = type;

            return values;
        }

        public void Dispose()
        {
            _args = null;
        }
    }
}

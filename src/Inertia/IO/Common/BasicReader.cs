﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Inertia
{
    public sealed class BasicReader : IDisposable
    {
        private static Dictionary<Type, BasicReturnAction<BasicReader, object>> _typageDefinitions = new Dictionary<Type, BasicReturnAction<BasicReader, object>>
        {
            { typeof(bool), (reader) => reader.GetBool() },
            { typeof(string), (reader) => reader.GetString() },
            { typeof(float), (reader) => reader.GetFloat() },
            { typeof(decimal), (reader) => reader.GetDecimal() },
            { typeof(double), (reader) => reader.GetDouble() },
            { typeof(byte), (reader) => reader.GetByte() },
            { typeof(sbyte), (reader) => reader.GetSByte() },
            { typeof(char), (reader) => reader.GetChar() },
            { typeof(short), (reader) => reader.GetShort() },
            { typeof(ushort), (reader) => reader.GetUShort() },
            { typeof(int), (reader) => reader.GetInt() },
            { typeof(uint), (reader) => reader.GetUInt() },
            { typeof(long), (reader) => reader.GetLong() },
            { typeof(ulong), (reader) => reader.GetULong() },
            { typeof(DateTime), (reader) => reader.GetDateTime() },
            { typeof(byte[]), (reader) => reader.GetBytes() },
        };

        public static void AddDeserializableType(Type type, BasicReturnAction<BasicReader, object> onDeserialize)
        {
            if (!_typageDefinitions.ContainsKey(type))
            {
                _typageDefinitions.Add(type, onDeserialize);
            }
            else
            {
                _typageDefinitions[type] = onDeserialize;
            }
        }

        public bool IsDisposed { get; private set; }
        public long TotalLength
        {
            get
            {
                return _reader != null ? _reader.Length : 0;
            }
        }
        public long UnreadedLength
        {
            get
            {
                return _reader != null ? (_reader.Length - _reader.Position) : 0;
            }
        }

        private MemoryStream _reader;
        private readonly Encoding _encoding;

        public BasicReader() : this(Encoding.UTF8)
        {
        }
        public BasicReader(Encoding encoding)
        {
            _encoding = encoding;
            _reader = new MemoryStream();
        }
        public BasicReader(byte[] data) : this(data, Encoding.UTF8)
        {
        }
        public BasicReader(byte[] data, Encoding encoding) : this(encoding)
        {
            Fill(data);
        }

        public BasicReader SetPosition(long position)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(BasicReader));
            }

            if (position < 0 || position > TotalLength) return this;

            _reader.Position = position;
            return this;
        }
        public long GetPosition()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(BasicReader));
            }

            return _reader.Position;
        }

        public void Clear()
        {
            if (!IsDisposed && _reader != null)
            {
                _reader.Dispose();
                _reader = new MemoryStream();
            }
        }

        public BasicReader Fill(ReadOnlySpan<byte> data)
        {
            return Fill(data, TotalLength);
        }
        public BasicReader Fill(ReadOnlySpan<byte> data, long offset)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(BasicReader));
            }

            var newLength = offset + data.Length;
            if (newLength > _reader.Length)
            {
                _reader.SetLength(newLength);
                _reader.Capacity = (int)newLength;
            }            

            var oldPosition = GetPosition();

            SetPosition(offset);
            _reader.Write(data);
            SetPosition(oldPosition);

            return this;
        }

        public BasicReader Skip(int length)
        {
            return SetPosition(GetPosition() + length);
        }
        public BasicReader RemoveReadedBytes()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(BasicReader));
            }

            var available = GetBytes((int)UnreadedLength);

            _reader.SetLength(available.Length);
            _reader.Capacity = available.Length;

            if (available.Length > 0)
            {
                Fill(available, 0);
                SetPosition(0);
            }

            return this;
        }

        public bool GetBool()
        {
            if (IsReadable(1))
            {
                return Convert.ToBoolean(_reader.ReadByte());
            }
            else return default;
        }
        public bool[] GetBoolFlag(int length)
        {
            return GetByte().ToBits(length);
        }
        public string GetString()
        {
            var data = GetBytes();
            if (data.Length > 0)
            {
                return _encoding.GetString(data);
            }
            else return string.Empty;
        }
        public byte GetByte()
        {
            if (IsReadable(1))
            {
                return (byte)_reader.ReadByte();
            }
            else return default;
        }
        public sbyte GetSByte()
        {
            if (IsReadable(1))
            {
                return unchecked((sbyte)_reader.ReadByte());
            }
            else return default;
        }
        public char GetChar()
        {
            if (TryReadSize(1, out var data))
            {
                return BitConverter.ToChar(data);
            }
            else return default;
        }
        public float GetFloat()
        {
            if (TryReadSize(4, out var data))
            {
                return BitConverter.ToSingle(data);
            }
            else return default;
        }
        public double GetDouble()
        {
            if (TryReadSize(8, out var data))
            {
                return BitConverter.ToDouble(data);
            }
            else return default;
        }
        public decimal GetDecimal()
        {
            if (TryReadSize(16, out var data))
            {
                var bits = new int[]
                {
                    BitConverter.ToInt32(data, 0),
                    BitConverter.ToInt32(data, 4),
                    BitConverter.ToInt32(data, 8),
                    BitConverter.ToInt32(data, 12)
                };

                return new decimal(bits);
            }
            else return default;
        }
        public short GetShort()
        {
            if (TryReadSize(2, out var data))
            {
                return BitConverter.ToInt16(data);
            }
            else return default;
        }
        public ushort GetUShort()
        {
            if (TryReadSize(2, out var data))
            {
                return BitConverter.ToUInt16(data);
            }
            else return default;
        }
        public int GetInt()
        {
            if (TryReadSize(4, out var data))
            {
                return BitConverter.ToInt32(data);
            }
            else return default;
        }
        public uint GetUInt()
        {
            if (TryReadSize(4, out var data))
            {
                return BitConverter.ToUInt32(data);
            }
            else return default;
        }
        public long GetLong()
        {
            if (TryReadSize(8, out var data))
            {
                return BitConverter.ToInt64(data);
            }
            else return default;
        }
        public ulong GetULong()
        {
            if (TryReadSize(8, out var data))
            {
                return BitConverter.ToUInt64(data);
            }
            else return default;
        }
        public DateTime GetDateTime()
        {
            return new DateTime(GetLong());
        }
        public byte[] GetBytes()
        {
            if (TryReadSize(4, out var data))
            {
                var length = BitConverter.ToInt32(data);
                return GetBytes(length);
            }
            else return new byte[0];
        }
        public byte[] GetBytes(int length)
        {
            if (TryReadSize(length, out var data))
            {
                return data;
            }
            else return new byte[0];
        }
        public T GetSerializableObject<T>() where T : ISerializableObject
        {
            return (T)GetSerializableObject(typeof(T));
        }
        public object GetSerializableObject(Type type)
        {
            if (!typeof(ISerializableObject).IsAssignableFrom(type)) return null;

            var parameters = type
                .GetConstructors()[0].GetParameters()
                .Select(p => null as object)
                .ToArray();
            var instance = Activator.CreateInstance(type, parameters);
            if (instance != null)
            {
                ((ISerializableObject)instance).Deserialize(this);
                return instance;
            }

            return null;
        }
        public T GetAutoSerializable<T>() where T : IAutoSerializable
        {
            return (T)GetAutoSerializable(typeof(T));
        }
        public void GetAutoSerializable<T>(T instance) where T : IAutoSerializable
        {
            GetAutoSerializable((IAutoSerializable)instance);
        }
        public IAutoSerializable GetAutoSerializable(Type type)
        {
            if (!typeof(IAutoSerializable).IsAssignableFrom(type))
            {
                throw new FriendlyException($"Type '{ type.Name }' is not '{ nameof(IAutoSerializable) }'");
            }

            var instance = Activator.CreateInstance(type);
            return GetAutoSerializable((IAutoSerializable)instance);
        }
        public IAutoSerializable GetAutoSerializable(IAutoSerializable instance)
        {
            if (ReflectionProvider.TryGetFields(instance.GetType(), out ReflectionProvider.SerializableFieldMemory[] fields))
            {
                foreach (var field in fields)
                {
                    field.Read(instance, this);
                }
            }

            return instance;
        }

        public T GetValue<T>()
        {
            return (T)GetValue(typeof(T));
        }
        public object GetValue(Type type)
        {
            if (_typageDefinitions.TryGetValue(type, out BasicReturnAction<BasicReader, object> action))
            {
                return action(this);
            }
            else
            {
                if (type.GetInterface(nameof(IAutoSerializable)) != null)
                {
                    return GetAutoSerializable(type);
                }
                else if (type.GetInterface(nameof(ISerializableObject)) != null)
                {
                    return GetSerializableObject(type);
                }
                else return null;
            }
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                _reader.Close();
                _reader.Dispose();

                IsDisposed = true;
            }
        }

        private bool IsReadable(int length)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(BasicReader));
            }

            return UnreadedLength >= length;
        }
        private bool TryReadSize(int length, out byte[] data)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(BasicReader));
            }

            if (UnreadedLength >= length)
            {
                data = new byte[length];
                _reader.Read(data);

                return true;
            }

            data = null;
            return false;
        }
    }
}
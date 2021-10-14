using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Inertia
{
    /// <summary>
    ///
    /// </summary>
    public class BasicReader : IDisposable
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
            { typeof(byte[]), (reader) => reader.GetBytes() },
        };

        /// <summary>
        /// Returns true is the current instance is disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }
        /// <summary>
        /// Returns the total length of the stream.
        /// </summary>
        public long TotalLength
        {
            get
            {
                if (_reader != null)
                {
                    return _reader.BaseStream.Length;
                }

                return 0;
            }
        }
        /// <summary>
        /// Returns the total unreaded length of the stream.
        /// </summary>
        public long UnreadedLength
        {
            get
            {
                if (_reader != null)
                {
                    return TotalLength - Position;
                }

                return 0;
            }
        }
        /// <summary>
        /// Get or Set the position in the stream.
        /// </summary>
        public long Position
        {
            get
            {
                if (_reader != null)
                {
                    return _reader.BaseStream.Position;
                }
                return 0;
            }
            set
            {
                if (_reader == null || value < 0 || value > _reader.BaseStream.Length)
                {
                    return;
                }

                _reader.BaseStream.Position = value;
            }
        }

        private BinaryReader _reader;
        private readonly Encoding _encoding;

        /// <summary>
        /// Initialize a new instance with empty data
        /// </summary>
        public BasicReader() : this(Encoding.UTF8)
        {
        }
        /// <summary>
        /// Initialize a new instance with empty data based on the specified <see cref="Encoding"/>
        /// </summary>
        /// <param name="encoding"><see cref="Encoding"/> for the reader</param>
        public BasicReader(Encoding encoding)
        {
            _encoding = encoding;
            _reader = new BinaryReader(new MemoryStream(), encoding);
        }
        /// <summary>
        /// Initialize a new instance with the specified data
        /// </summary>
        /// <param name="data">The target byte array</param>
        public BasicReader(byte[] data) : this(data, Encoding.UTF8)
        {
        }
        /// <summary>
        /// Initialize a new instance with the specified data based on the specified <see cref="Encoding"/>
        /// </summary>
        /// <param name="data">Data to read</param>
        /// <param name="encoding"><see cref="Encoding"/> for the reader</param>
        public BasicReader(byte[] data, Encoding encoding) : this(encoding)
        {
            Fill(data);
        }

        /// <summary>
        /// Clear the current stream.
        /// </summary>
        public void Clear()
        {
            if (!IsDisposed && _reader != null)
            {
                _reader.Dispose();
                _reader = new BinaryReader(new MemoryStream(), _encoding);
            }
        }

        /// <summary>
        /// Fill the current stream with the specified data
        /// </summary>
        /// <param name="data">Data to add</param>
        /// <returns></returns>
        public BasicReader Fill(byte[] data)
        {
            return Fill(data, TotalLength);
        }
        /// <summary>
        /// Fill the current stream with the specified data starting at the specified index
        /// </summary>
        /// <param name="data">Data to add</param>
        /// <param name="startIndex">Start index in current stream</param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException"></exception>
        public BasicReader Fill(byte[] data, long startIndex)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(BasicReader));
            }

            var oldPosition = Position;

            Position = startIndex;
            _reader.BaseStream.Write(data, 0, data.Length);
            Position = oldPosition;

            return this;
        }

        /// <summary>
        /// Remove all the readed data in the stream and refresh the stream with the non-readed data
        /// </summary>
        /// <returns></returns>
        public BasicReader RemoveReadedBytes()
        {
            var available = GetBytes(UnreadedLength);
            _reader.BaseStream.SetLength(available.Length);

            if (available.Length > 0)
            {
                Fill(available, 0);
                Position = 0;
            }

            return this;
        }

        /// <summary>
        /// Read a <see cref="bool"/> value in the stream and change the position
        /// </summary>
        /// <returns>Readed <see cref="bool"/> value or false if nothing can be read</returns>
        public bool GetBool()
        {
            if (IsUpdatable(sizeof(bool)))
            {
                return _reader.ReadBoolean();
            }
            else
            {
                return default;
            }
        }
        /// <summary>
        /// Read a bool flag based on specified length
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public bool[] GetBoolFlag(int length)
        {
            return GetByte().GetBits(length);
        }

        /// <summary>
        /// Read a <see cref="string"/> value with the current instance <see cref="Encoding"/> algorithm in the stream and change the position
        /// </summary>
        /// <returns>Readed <see cref="string"/> value or <see cref="string.Empty"/> if nothing can be read</returns>
        public string GetString()
        {
            var b = GetBytes();
            if (b.Length > 0)
            {
                return _encoding.GetString(b);
            }
            else
            {
                return string.Empty;
            }
        }
        /// <summary>
        /// Read a <see cref="byte"/> value in the stream and change the position
        /// </summary>
        /// <returns>Readed <see cref="byte"/> value or 0 if nothing can be read</returns>
        public byte GetByte()
        {
            if (IsUpdatable(sizeof(byte)))
            {
                return _reader.ReadByte();
            }
            else
            {
                return default;
            }            
        }
        /// <summary>
        /// Read a <see cref="sbyte"/> value in the stream and change the position
        /// </summary>
        /// <returns>Readed <see cref="sbyte"/> value or 0 if nothing can be read</returns>
        public sbyte GetSByte()
        {
            if (IsUpdatable(sizeof(sbyte)))
            {
                return _reader.ReadSByte();
            }
            else
            {
                return default;
            }
        }
        /// <summary>
        /// Read a <see cref="char"/> value in the stream and change the position
        /// </summary>
        /// <returns>Readed <see cref="char"/> value or default <see cref="char"/> if nothing can be read</returns>
        public char GetChar()
        {
            if (IsUpdatable(sizeof(char)))
            {
                return _reader.ReadChar();
            }
            else
            {
                return default;
            }
        }
        /// <summary>
        /// Read a <see cref="float"/> value in the stream and change the position
        /// </summary>
        /// <returns>Readed <see cref="float"/> value or 0 if nothing can be read</returns>
        public float GetFloat()
        {
            if (IsUpdatable(sizeof(float)))
            {
                return _reader.ReadSingle();
            }
            else
            {
                return default;
            }
        }
        /// <summary>
        /// Read a <see cref="double"/> value in the stream and change the position
        /// </summary>
        /// <returns>Readed <see cref="double"/> value or 0 if nothing can be read</returns>
        public double GetDouble()
        {
            if (IsUpdatable(sizeof(double)))
            {
                return _reader.ReadDouble();
            }
            else
            {
                return default;
            }
        }
        /// <summary>
        /// Read a <see cref="decimal"/> value in the stream and change the position
        /// </summary>
        /// <returns>Readed <see cref="decimal"/> value or 0 if nothing can be read</returns>
        public decimal GetDecimal()
        {
            if (IsUpdatable(sizeof(decimal)))
            {
                return _reader.ReadDecimal();
            }
            else
            {
                return default;
            }
        }
        /// <summary>
        /// Read a <see cref="short"/> value in the stream and change the position
        /// </summary>
        /// <returns>Readed <see cref="short"/> value or 0 if nothing can be read</returns>
        public short GetShort()
        {
            if (IsUpdatable(sizeof(short)))
            {
                return _reader.ReadInt16();
            }
            else
            {
                return default;
            }
        }
        /// <summary>
        /// Read a <see cref="ushort"/> value in the stream and change the position
        /// </summary>
        /// <returns>Readed <see cref="ushort"/> value or 0 if nothing can be read</returns>
        public ushort GetUShort()
        {
            if (IsUpdatable(sizeof(ushort)))
            {
                return _reader.ReadUInt16();
            }
            else
            {
                return default;
            }
        }
        /// <summary>
        /// Read a <see cref="int"/> value in the stream and change the position
        /// </summary>
        /// <returns>Readed <see cref="int"/> value or 0 if nothing can be read</returns>
        public int GetInt()
        {
            if (IsUpdatable(sizeof(int)))
            {
                return _reader.ReadInt32();
            }
            else
            {
                return default;
            }
        }
        /// <summary>
        /// Read a <see cref="uint"/> value in the stream and change the position
        /// </summary>
        /// <returns>Readed <see cref="uint"/> value or 0 if nothing can be read</returns>
        public uint GetUInt()
        {
            if (IsUpdatable(sizeof(uint)))
            {
                return _reader.ReadUInt32();
            }
            else
            {
                return default;
            }            
        }
        /// <summary>
        /// Read a <see cref="long"/> value in the stream and change the position
        /// </summary>
        /// <returns>Readed <see cref="long"/> value or 0 if nothing can be read</returns>
        public long GetLong()
        {
            if (IsUpdatable(sizeof(long)))
            {
                return _reader.ReadInt64();
            }
            else
            {
                return default;
            }            
        }
        /// <summary>
        /// Read a <see cref="ulong"/> value in the stream and change the position
        /// </summary>
        /// <returns>Readed <see cref="ulong"/> value or 0 if nothing can be read</returns>
        public ulong GetULong()
        {
            if (IsUpdatable(sizeof(ulong)))
            {
                return _reader.ReadUInt64();
            }
            else
            {
                return default;
            }            
        }
        /// <summary>
        /// Read a byte array (with an <see cref="long"/> length header) in the stream and change the position
        /// </summary>
        /// <returns>Readed byte array value or empty byte array if nothing can be read</returns>
        public byte[] GetBytes()
        {
            if (IsUpdatable(sizeof(uint)))
            {
                var length = GetUInt();
                return GetBytes(length);
            }
            else
            {
                return new byte[0];
            }
        }
        /// <summary>
        /// Read specified number of <see cref="byte"/> in the stream and change the position
        /// </summary>
        /// <param name="length">Length ot the data's buffer</param>
        /// <returns>Readed byte array of specified length or an empty byte array if nothing can be read</returns>
        public byte[] GetBytes(long length)
        {
            if (IsUpdatable(length))
            {
                return _reader.ReadBytes((int)length);
            }
            else
            {
                return new byte[0];
            }
        }
        /// <summary>
        /// Read DateTime in the stream and change the position
        /// </summary>
        /// <returns>Returns a <see cref="DateTime"/> instance</returns>
        public DateTime GetDateTime()
        {
            return new DateTime(GetLong());
        }
        /// <summary>
        /// Create an instance of <typeparamref name="T"/> and return it after deserialization
        /// </summary>
        /// <returns>Returns a <see cref="ISerializableObject"/></returns>
        public T TryDeserializeObject<T>() where T : ISerializableObject
        {
            var parameters = typeof(T)
                .GetConstructors()[0].GetParameters()
                .Select(p => (object)null)
                .ToArray();
            var instance = (T)Activator.CreateInstance(typeof(T), parameters);
            if (instance != null)
            {
                instance.Deserialize(this);
                return instance;
            }

            return default(T);
        }
        /// <summary>
        /// Create an instance of <typeparamref name="T"/> and return it after deserialization
        /// </summary>
        /// <returns>Returns a <see cref="ISerializableData"/></returns>
        public T TryDeserializeData<T>() where T : ISerializableData
        {
            var parameters = typeof(T)
                .GetConstructors()[0].GetParameters()
                .Select(p => (object)null)
                .ToArray();
            var instance = (T)Activator.CreateInstance(typeof(T), parameters);
            if (instance != null)
            {
                instance.Deserialize(GetBytes());
                return instance;
            }

            return default(T);
        }
        /// <summary>
        /// Read the next <typeparamref name="T"/> object in the stream having a <see cref="SerializableAttribute"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>Deserialized instance of <typeparamref name="T"/></returns>
        public T GetObject<T>()
        {
            return (T)GetObject();
        }
        /// <summary>
        /// Read the next object in the stream having a <see cref="SerializableAttribute"/>
        /// </summary>
        /// <returns>Deserialized object</returns>
        public object GetObject()
        {
            return new BinaryFormatter
            {
                TypeFormat = System.Runtime.Serialization.Formatters.FormatterTypeStyle.TypesWhenNeeded
            }.Deserialize(_reader.BaseStream);
        }

        public IAutoSerializable GetAuto(Type type)
        {
            if (type.GetInterface(nameof(IAutoSerializable)) == null)
            {
                throw new Exception($"Type '{ type.Name }' isn't '{ nameof(IAutoSerializable) }'");
            }

            var instance = Activator.CreateInstance(type);
            var fields = instance.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

            foreach (var field in fields)
            {
                var ignored = field.GetCustomAttribute<IgnoreField>() != null;
                if (!ignored)
                {
                    if (field.FieldType.GetInterface(nameof(IAutoSerializable)) != null)
                    {
                        field.SetValue(instance, GetAuto(field.FieldType));
                    }
                    else
                    {
                        var customization = field.GetCustomAttribute<CustomDeserialization>();
                        if (customization == null)
                        {
                            field.SetValue(instance, GetTypedValue(field.FieldType));
                        }
                        else
                        {
                            var method = type.GetMethod($"Deserialize{ field.Name }", BindingFlags.NonPublic | BindingFlags.Instance);
                            if (method != null)
                            {
                                var parameters = method.GetParameters();
                                if (parameters.Length == 1 && parameters[0].ParameterType == typeof(BasicReader))
                                {
                                    method.Invoke(instance, new object[] { this });
                                }
                            }
                        }
                    }
                }
            }

            return (IAutoSerializable)instance;
        }

        public T GetAuto<T>() where T : IAutoSerializable
        {
            return (T)GetAuto(typeof(T));
        }

        public object GetTypedValue(Type type)
        {
            if (_typageDefinitions.ContainsKey(type))
            {
                return _typageDefinitions[type](this);
            }
            else
            {
                return GetObject();
            }
        }
        /// <summary>
        /// Read the next object in the stream based on the specified <see cref="Type"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetTypedValue<T>()
        {
            return (T)GetTypedValue(typeof(T));
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
            if (!IsDisposed && disposing)
            {
                if (disposing)
                {
                    _reader.Close();
                    _reader.Dispose();
                }

                IsDisposed = true;
            }
        }

        private bool IsUpdatable(long length)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(BasicReader));
            }

            return UnreadedLength >= length;
        }
    }
}

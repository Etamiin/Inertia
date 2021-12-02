using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
<<<<<<< HEAD
<<<<<<< HEAD
using System.Runtime.Serialization.Formatters.Binary;
=======
using System.Reflection;
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
=======
using System.Reflection;
>>>>>>> premaster
using System.Text;

namespace Inertia
{
<<<<<<< HEAD
<<<<<<< HEAD
    /// <summary>
    ///
    /// </summary>
    public class BasicReader : IDisposable
    {
        /// <summary>
        /// Returns true is the current instance is disposed.
        /// </summary>
=======
    public class BasicReader : IDisposable
=======
    public sealed class BasicReader : IDisposable
>>>>>>> premaster
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

        public static void SetTypeDeserialization(Type type, BasicReturnAction<BasicReader, object> deserialization)
        {
            if (!_typageDefinitions.ContainsKey(type))
            {
                _typageDefinitions.Add(type, deserialization);
            }
            else
            {
                _typageDefinitions[type] = deserialization;
            }
        }

<<<<<<< HEAD
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
=======
>>>>>>> premaster
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

<<<<<<< HEAD
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
=======
        public BasicReader() : this(Encoding.UTF8)
        {
        }
>>>>>>> premaster
        public BasicReader(Encoding encoding)
        {
            _encoding = encoding;
            _reader = new BinaryReader(new MemoryStream(), encoding);
        }
<<<<<<< HEAD
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
=======
        public BasicReader(byte[] data) : this(data, Encoding.UTF8)
        {
        }
>>>>>>> premaster
        public BasicReader(byte[] data, Encoding encoding) : this(encoding)
        {
            Fill(data);
        }

<<<<<<< HEAD
<<<<<<< HEAD
        /// <summary>
        /// Clear the current stream.
        /// </summary>
=======
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
=======
>>>>>>> premaster
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

<<<<<<< HEAD
<<<<<<< HEAD
        /// <summary>
        /// Remove all the readed data in the stream and refresh the stream with the non-readed data
        /// </summary>
        /// <returns></returns>
=======
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
=======
>>>>>>> premaster
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
<<<<<<< HEAD
<<<<<<< HEAD

=======
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
=======
>>>>>>> premaster
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
<<<<<<< HEAD
<<<<<<< HEAD
            if (IsUpdatable(sizeof(long)))
            {
                var length = GetLong();
=======
            if (IsUpdatable(sizeof(uint)))
            {
                var length = GetUInt();
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
=======
            if (IsUpdatable(sizeof(uint)))
            {
                var length = GetUInt();
>>>>>>> premaster
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
<<<<<<< HEAD
<<<<<<< HEAD
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
=======
        public T GetSerializableObject<T>() where T : ISerializableObject
        {
            return (T)GetSerializableObject(typeof(T));
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
=======
        public T GetSerializableObject<T>() where T : ISerializableObject
        {
            return (T)GetSerializableObject(typeof(T));
>>>>>>> premaster
        }
        /// <summary>
        /// Create an instance of <typeparamref name="T"/> and return it after deserialization
        /// </summary>
<<<<<<< HEAD
<<<<<<< HEAD
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
=======
=======
>>>>>>> premaster
        /// <returns>Returns a <see cref="ISerializableObject"/></returns>
        public object GetSerializableObject(Type type)
        {
            var parameters = type
                .GetConstructors()[0].GetParameters()
                .Select(p => (object)null)
                .ToArray();
            var instance = Activator.CreateInstance(type, parameters);
            if (instance != null)
            {
                ((ISerializableObject)instance).Deserialize(this);
                return instance;
            }

            return null;
        }
        /// <summary>
        /// Create an instance of <typeparamref name="T"/> deserialize it and then return it
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetAutoSerializable<T>() where T : IAutoSerializable
        {
            return (T)GetAutoSerializable(typeof(T));
        }
        /// <summary>
        /// Deserialize the specified instance of <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public void GetAutoSerializable<T>(T instance) where T : IAutoSerializable
        {
            GetAutoSerializable((IAutoSerializable)instance);
        }
        /// <summary>
        /// Create an instance of specified <see cref="IAutoSerializable"/> deserialize it and then return it
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IAutoSerializable GetAutoSerializable(Type type)
        {
            if (type.GetInterface(nameof(IAutoSerializable)) == null)
            {
                throw new Exception($"Type '{ type.Name }' isn't '{ nameof(IAutoSerializable) }'");
            }

            var instance = Activator.CreateInstance(type);
            return GetAutoSerializable((IAutoSerializable)instance);
        }
        /// <summary>
        /// Deserialize the specified instance of <see cref="IAutoSerializable"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IAutoSerializable GetAutoSerializable(IAutoSerializable instance)
        {
            var type = instance.GetType();
            var fields = instance.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance).OrderBy((x) => x.MetadataToken);

            foreach (var field in fields)
            {
                if (field.GetCustomAttribute<IgnoreField>() == null)
                {
                    if (!typeof(IAutoSerializable).IsAssignableFrom(field.FieldType))
                    {
                        var customization = field.GetCustomAttribute<CustomDeserialization>();
                        if (customization == null)
                        {
                            field.SetValue(instance, GetValue(field.FieldType));
                        }
                        else
                        {
                            var method = type.GetMethod(customization.MethodName, BindingFlags.NonPublic | BindingFlags.Instance);
                            var parameters = method?.GetParameters();

                            if (parameters.Length == 1 && parameters[0].ParameterType == typeof(BasicReader))
                            {
                                method.Invoke(instance, new object[] { this });
                            }
                        }
                    }
                    else
                    {
                        field.SetValue(instance, GetAutoSerializable(field.FieldType));
                    }
                }
            }

            return instance;
        }

        /// <summary>
        /// Read the next object in the stream based on the specified <see cref="Type"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetValue<T>()
        {
            return (T)GetValue(typeof(T));
        }
        /// <summary>
        /// Read the next object in the stream based on the specified <see cref="Type"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public object GetValue(Type type)
        {
            if (_typageDefinitions.ContainsKey(type))
            {
                return _typageDefinitions[type](this);
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
                else
                {
                    return null;
                }
            }
        }

        public void Dispose()
        {
<<<<<<< HEAD
            Dispose(true);
        }
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed && disposing)
            {
                if (disposing)
                {
                    _reader.Close();
                    _reader.Dispose();
                }
=======
            if (!IsDisposed)
            {
                _reader.Close();
                _reader.Dispose();
>>>>>>> premaster

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
<<<<<<< HEAD
<<<<<<< HEAD
}
=======
}
>>>>>>> 9bfc85f6784b254a10c65f104446a83c8b195c40
=======
}
>>>>>>> premaster

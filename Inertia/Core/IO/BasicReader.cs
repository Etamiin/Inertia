using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Inertia
{
    /// <summary>
    /// Provides read-only data management
    /// </summary>
    public class BasicReader : IDisposable
    {
        #region Static variables

        internal static Dictionary<Type, BasicReturnAction<BasicReader, object>> TypageDefinitons { get; private set; }

        #endregion

        #region Public variables

        /// <summary>
        /// Return true is the current instance is disposed
        /// </summary>
        public bool IsDisposed { get; private set; }
        /// <summary>
        /// Get the total length currently in the stream
        /// </summary>
        public long TotalLength
        {
            get
            {
                if (m_reader == null)
                    return 0;

                return m_reader.BaseStream.Length;
            }
        }
        /// <summary>
        /// Get the total length unreaded (based on current position in the stream)
        /// </summary>
        public long UnreadedLength
        {
            get
            {
                if (m_reader == null)
                    return 0;

                return TotalLength - Position;
            }
        }
        /// <summary>
        /// Get or set the current position in the stream
        /// </summary>
        public long Position
        {
            get
            {
                if (m_reader == null )
                    return 0;

                return m_reader.BaseStream.Position;
            }
            set
            {
                if (value < 0 || value > m_reader.BaseStream.Length)
                    return;

                m_reader.BaseStream.Position = value;
            }
        }

        #endregion

        #region Private variables

        private BinaryReader m_reader;
        private Encoding m_encoding;

        #endregion

        #region Constructors

        /// <summary>
        /// Create a new instance with empty data
        /// </summary>
        public BasicReader() : this(Encoding.UTF8)
        {
        }
        /// <summary>
        /// Create a new instance with empty data based on the specified <see cref="Encoding"/>
        /// </summary>
        /// <param name="encoding">Target <see cref="Encoding"/> for the reader</param>
        public BasicReader(Encoding encoding)
        {
            m_encoding = encoding;
            m_reader = new BinaryReader(new MemoryStream(), encoding);

            if (TypageDefinitons != null)
                return;

            TypageDefinitons = new Dictionary<Type, BasicReturnAction<BasicReader, object>>()
            {
                {  typeof(bool), (reader) => reader.GetBool() },
                {  typeof(string), (reader) => reader.GetString() },
                {  typeof(byte), (reader) => reader.GetByte() },
                {  typeof(sbyte), (reader) => reader.GetSByte() },
                {  typeof(char), (reader) => reader.GetChar() },
                {  typeof(short), (reader) => reader.GetShort() },
                {  typeof(ushort), (reader) => reader.GetUShort() },
                {  typeof(float), (reader) => reader.GetFloat() },
                {  typeof(double), (reader) => reader.GetDouble() },
                {  typeof(decimal), (reader) => reader.GetDecimal() },
                {  typeof(int), (reader) => reader.GetInt() },
                {  typeof(uint), (reader) => reader.GetUInt() },
                {  typeof(long), (reader) => reader.GetLong() },
                {  typeof(ulong), (reader) => reader.GetULong() },
                {  typeof(byte[]), (reader) => reader.GetBytes() }
            };
        }
        /// <summary>
        /// Create a new instance with the specified byte array data
        /// </summary>
        /// <param name="data">The target byte array</param>
        public BasicReader(byte[] data) : this(data, Encoding.UTF8)
        {
        }
        /// <summary>
        /// Create a new instance with the specified byte array data based on the specified <see cref="Encoding"/>
        /// </summary>
        /// <param name="data">The target byte array</param>
        /// <param name="encoding">Target <see cref="Encoding"/> for the reader</param>
        public BasicReader(byte[] data, Encoding encoding) : this(encoding)
        {
            Fill(data);
        }

        #endregion

        /// <summary>
        /// Clear the current instance and create a new empty stream
        /// </summary>
        public void Clear()
        {
            if (IsDisposed)
                return;

            if (m_reader == null)
                return;

            m_reader.Dispose();
            m_reader = new BinaryReader(new MemoryStream(), m_encoding);
        }
        
        /// <summary>
        /// Fill the current stream with the specified byte array data (added at the end)
        /// </summary>
        /// <param name="data">Target byte array data to add</param>
        /// <returns></returns>
        public BasicReader Fill(byte[] data)
        {
            return Fill(data, TotalLength);
        }
        /// <summary>
        /// Fill the current stream with the specified byte array data (added at the end)
        /// </summary>
        /// <param name="data">Target byte array data to add</param>
        /// <param name="startIndex">Start index</param>
        /// <returns></returns>
        public BasicReader Fill(byte[] data, long startIndex)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(BasicReader));

            var oldPosition = Position;

            Position = startIndex;
            m_reader.BaseStream.Write(data, 0, data.Length);

            Position = oldPosition;

            return this;
        }

        /// <summary>
        /// Remove all the readed bytes in the stream (before the current position) and refresh the stream with the non-readed bytes
        /// </summary>
        /// <returns></returns>
        public BasicReader RemoveReadedBytes()
        {
            var available = GetBytes(UnreadedLength);
            m_reader.BaseStream.SetLength(available.Length);
            
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
        /// <returns>Readed <see cref="bool"/> value (or false if nothing can be read)</returns>
        public bool GetBool()
        {
            if (IsUpdatable(sizeof(bool)))
                return m_reader.ReadBoolean();

            return default;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public bool[] GetBoolFlag(int length)
        {
            var flags = new bool[length];
            var flag = GetByte();

            for (var i = 0; i < length; i++)
                flags[i] = (flag & (byte)(1 << i)) != 0;

            return flags;
        }

        /// <summary>
        /// Read a <see cref="string"/> value with the current instance <see cref="Encoding"/> algorithm in the stream and change the position
        /// </summary>
        /// <returns>Readed string value (or <see cref="string.Empty"/> if nothing can be read)</returns>
        public string GetString()
        {
            return m_encoding.GetString(GetBytes());
        }
        /// <summary>
        /// Read a <see cref="byte"/> value in the stream and change the position
        /// </summary>
        /// <returns>Readed <see cref="byte"/> value (or 0 if nothing can be read)</returns>
        public byte GetByte()
        {
            if (IsUpdatable(sizeof(byte)))
                return m_reader.ReadByte();

            return default;
        }
        /// <summary>
        /// Read a <see cref="sbyte"/> value in the stream and change the position
        /// </summary>
        /// <returns>Readed <see cref="sbyte"/> value (or 0 if nothing can be read)</returns>
        public sbyte GetSByte()
        {
            if (IsUpdatable(sizeof(sbyte)))
                return m_reader.ReadSByte();

            return default;
        }
        /// <summary>
        /// Read a <see cref="char"/> value in the stream and change the position
        /// </summary>
        /// <returns>Readed <see cref="char"/> value (or (char)0 if nothing can be read)</returns>
        public char GetChar()
        {
            if (IsUpdatable(sizeof(char)))
                return m_reader.ReadChar();
            return default;
        }
        /// <summary>
        /// Read a <see cref="short"/> value in the stream and change the position
        /// </summary>
        /// <returns>Readed <see cref="short"/> value (or 0 if nothing can be read)</returns>
        public short GetShort()
        {
            if (IsUpdatable(sizeof(short)))
                return m_reader.ReadInt16();

            return default;
        }
        /// <summary>
        /// Read a <see cref="ushort"/> value in the stream and change the position
        /// </summary>
        /// <returns>Readed <see cref="ushort"/> value (or 0 if nothing can be read)</returns>
        public ushort GetUShort()
        {
            if (IsUpdatable(sizeof(ushort)))
                return m_reader.ReadUInt16();

            return default;
        }
        /// <summary>
        /// Read a <see cref="float"/> value in the stream and change the position
        /// </summary>
        /// <returns>Readed <see cref="float"/> value (or 0 if nothing can be read)</returns>
        public float GetFloat()
        {
            if (IsUpdatable(sizeof(float)))
                return m_reader.ReadSingle();

            return default;
        }
        /// <summary>
        /// Read a <see cref="double"/> value in the stream and change the position
        /// </summary>
        /// <returns>Readed <see cref="double"/> value (or 0 if nothing can be read)</returns>
        public double GetDouble()
        {
            if (IsUpdatable(sizeof(double)))
                return m_reader.ReadDouble();

            return default;
        }
        /// <summary>
        /// Read a <see cref="decimal"/> value in the stream and change the position
        /// </summary>
        /// <returns>Readed <see cref="decimal"/> value (or 0 if nothing can be read)</returns>
        public decimal GetDecimal()
        {
            if (IsUpdatable(sizeof(decimal)))
                return m_reader.ReadDecimal();

            return default;
        }
        /// <summary>
        /// Read a <see cref="int"/> value in the stream and change the position
        /// </summary>
        /// <returns>Readed <see cref="int"/> value (or 0 if nothing can be read)</returns>
        public int GetInt()
        {
            if (IsUpdatable(sizeof(int)))
                return m_reader.ReadInt32();

            return default;
        }
        /// <summary>
        /// Read a <see cref="uint"/> value in the stream and change the position
        /// </summary>
        /// <returns>Readed <see cref="uint"/> value (or 0 if nothing can be read)</returns>
        public uint GetUInt()
        {
            if (IsUpdatable(sizeof(uint)))
                return m_reader.ReadUInt32();

            return default;
        }
        /// <summary>
        /// Read a <see cref="long"/> value in the stream and change the position
        /// </summary>
        /// <returns>Readed <see cref="long"/> value (or 0 if nothing can be read)</returns>
        public long GetLong()
        {
            if (IsUpdatable(sizeof(long)))
                return m_reader.ReadInt64();

            return default;
        }
        /// <summary>
        /// Read a <see cref="ulong"/> value in the stream and change the position
        /// </summary>
        /// <returns>Readed <see cref="ulong"/> value (or 0 if nothing can be read)</returns>
        public ulong GetULong()
        {
            if (IsUpdatable(sizeof(ulong)))
                return m_reader.ReadUInt64();

            return default;
        }
        /// <summary>
        /// Read a byte array (with an <see cref="int"/> length header) in the stream and change the position
        /// </summary>
        /// <returns>Readed <see cref="short"/> value (or 0 if nothing can be read)</returns>
        public byte[] GetBytes()
        {
            if (IsUpdatable(sizeof(long)))
            {
                var length = GetLong();
                return GetBytes(length);
            }

            return new byte[] { };
        }
        /// <summary>
        /// Read specified number of <see cref="byte"/> in the stream and change the position
        /// </summary>
        /// <param name="length">Length ot the byte array result</param>
        /// <returns>Complete byte array of specified length (or an empty byte array if the <see cref="UnreadedLength"/> is too small)</returns>
        public byte[] GetBytes(long length)
        {
            if (IsUpdatable(length))
                return m_reader.ReadBytes((int)length);

            return new byte[] { };
        }
        /// <summary>
        /// Read DateTime in the stream and change the position
        /// </summary>
        /// <returns>Return a DateTime instance</returns>
        public DateTime GetDateTime()
        {
            return new DateTime(GetLong());
        }
        /// <summary>
        /// Deserializa the specified <see cref="ISerializableObject"/> object with the custom serialization method
        /// </summary>
        /// <param name="instance">Instance where to deserialize data</param>
        /// <returns>Return a <see cref="ISerializableObject"/></returns>
        public ISerializableObject DeserializeObject(ISerializableObject instance)
        {
            instance.Deserialize(this);
            return instance;
        }
        /// <summary>
        /// Create an instance and return it after deserialization
        /// </summary>
        /// <returns>Return a <see cref="ISerializableObject"/></returns>
        public T DeserializeObject<T>() where T : ISerializableObject
        {
            try
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
            }
            catch { }

            return default(T);
        }
        /// <summary>
        /// Deserializa the specified <see cref="ISerializableObject"/> objects with the custom serialization method
        /// </summary>
        /// <param name="instances">Instances where to deserialize data</param>
        /// <returns>Return a <see cref="ISerializableObject"/> <see cref="IEnumerable"/> object</returns>
        public IEnumerable<ISerializableObject> DeserializeObjects(IEnumerable<ISerializableObject> instances)
        {
            foreach (var instance in instances)
                instance.Deserialize(this);

            return instances;
        }
        /// <summary>
        /// Create instances and return them after deserialization
        /// </summary>
        /// <returns>Return a <see cref="ISerializableObject"/></returns>
        public IEnumerable<T> DeserializeObjects<T>(int count) where T : ISerializableObject
        {
            var instances = new T[count];
            for (var i = 0; i < instances.Length; i++)
            {
                var parameters = typeof(T)
                    .GetConstructors()[0].GetParameters()
                    .Select(p => (object)null)
                    .ToArray();

                instances[i] = (T)Activator.CreateInstance(typeof(T), parameters);
                instances[i].Deserialize(this);
            }

            return instances;
        }
        /// <summary>
        /// Read the specified <typeparamref name="T"/> in the stream (only if the type has a <see cref="SerializableAttribute"/>)
        /// </summary>
        /// <typeparam name="T">Target type to deserialize</typeparam>
        /// <returns>An deserialized instance of the specified Type</returns>
        public T GetObject<T>()
        {
            return (T)GetObject();
        }
        /// <summary>
        /// Read the next object in the stream (only if the type has a <see cref="SerializableAttribute"/>)
        /// </summary>
        /// <returns>An deserialized object</returns>
        public object GetObject()
        {
            var binaryFormatter = new BinaryFormatter()
            {
                TypeFormat = System.Runtime.Serialization.Formatters.FormatterTypeStyle.TypesWhenNeeded
            };

            return binaryFormatter.Deserialize(m_reader.BaseStream);
        }
        /// <summary>
        /// Read the specified <see cref="Type"/> and return it (if it's readable, or null)
        /// </summary>
        /// <param name="valueType">The type of the target object</param>
        /// <returns>Return the readed value (or null)</returns>
        public object GetValue(Type valueType)
        {
            if (TypageDefinitons.ContainsKey(valueType))
                return TypageDefinitons[valueType](this);

            if (valueType.IsSerializable)
                return GetObject<object>();

            return null;
        }
        /// <summary>
        /// Read all object corresponding to the specified <see cref="Type"/> in the array (null if unreadable)
        /// </summary>
        /// <param name="valuesType">Array of <see cref="Type"/> to read</param>
        /// <returns><see cref="IList"/> of all objects readed (can contains null elements)</returns>
        public IList GetValues(params Type[] valuesType)
        {
            var values = new object[valuesType.Length];

            var i = 0;
            foreach (var type in valuesType)
                values[i++] = GetValue(type);

            return values;
        }

        /// <summary>
        /// Dispose the reader (can't be used anymore)
        /// </summary>
        public void Dispose()
        {
            if (IsDisposed)
                return;

            m_reader.Close();
            m_reader.Dispose();
            m_reader = null;
            m_encoding = null;
            IsDisposed = true;
        }

        private bool IsUpdatable(long length)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(BasicReader));
            return UnreadedLength >= length;
        }
    }
}

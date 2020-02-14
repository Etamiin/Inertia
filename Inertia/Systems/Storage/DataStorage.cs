using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inertia.Internal;

namespace Inertia.Storage
{
    public class DataStorage<T> : InertiaStorage
    {
        #region Events

        public event DataStorageCompletedHandler<T> Saved = (storage) => { };
        public event DataStorageCompletedHandler<T> Loaded = (storage) => { };
        public event DataStorageProgressFailedHandler<T> SaveFailed = (storage, exception) => { };
        public event DataStorageProgressFailedHandler<T> LoadFailed = (storage, exception) => { };
        public event DataStorageProgressHandler<T> SaveProgress = (storage, progression) => { };
        public event DataStorageProgressHandler<T> LoadProgress = (storage, progression) => { };

        #endregion

        #region Static methods

        public static DataStorage<T> Create()
        {
            return new DataStorage<T>("Unknown");
        }
        public static DataStorage<T> Create(string name)
        {
            return new DataStorage<T>(name);
        }

        #endregion

        #region Public variables

        public override int Count => Container.Count;

        #endregion

        #region Private variables

        private FlexDictionary<T> Container;

        #endregion

        #region Constructors

        internal DataStorage(string name)
        {
            if (typeof(T).ToTypeCode() == TypeCode.Object)
                throw new InvalidKeyTypeException(typeof(T));

            Container = new FlexDictionary<T>();

            Name = name;
        }

        #endregion

        public object this[T key]
        {
            get
            {
                return Container[key];
            }
            set
            {
                Remove(key);
                Add(key, value);
            }
        }
        
        public T[] GetKeys()
        {
            return Container.GetKeys();
        }

        public DataStorage<T> Add(T key, object value)
        {
            if (CheckObject(ref value))
                Container.Add(key, value);

            return this;
        }
        public DataStorage<T> Remove(T key)
        {
            Container.Remove(key);
            return this;
        }
        public DataStorage<T> Replace(T key, object value)
        {
            if (CheckObject(ref value))
                Container.Replace(key, value);

            return this;
        }

        public bool TryGetValue<TData>(T key, out TData value)
        {
            return Container.TryGetValue(key, out value);
        }
        public TData GetValue<TData>(T key)
        {
            return Container.GetValue<TData>(key);
        }
        
        public bool Exist(T key)
        {
            return Container.Exist(key);
        }
        public bool IsOfType<TData>(T identifier)
        {
            return Container.IsOfType<TData>(identifier);
        }

        private bool CheckObject(ref object value)
        {
            if (value.GetType().ToTypeCode() == TypeCode.Object)
            {
                if (value is Array && value.GetType().ToArrayTypeCode() == ArrayTypeCode.InvalidType ||
                    !(value is ISerializableObject) && !(value is Array))
                    throw new InvalidDataTypeException(value);
            }

            return true;
        }
        
        internal override byte[] Serialize(bool async)
        {
            void SetArray(object value, InertiaWriter writer)
            {
                var vArray = (Array)value;

                writer
                    .SetBool(true)
                    .SetInt(vArray.Length)
                    .SetByte((byte)value.GetType().ToArrayTypeCode());

                for (var i = 0; i < vArray.Length; i++)
                {
                    var _v = vArray.GetValue(i);
                    var _vc = _v.GetType().ToTypeCode();

                    writer.SetByte((byte)_vc);
                    if (_vc == TypeCode.Object)
                    {
                        if (_v is Array)
                            SetArray(_v, writer);
                        else
                            SetObject(_v, writer);
                    }
                    else
                        writer.SetValue(_v);
                }
            }
            void SetObject(object value, InertiaWriter writer)
            {
                writer
                    .SetBool(false)
                    .SetString(value.GetType().Name);

                ((ISerializableObject)value).Serialize(ref writer);
            }

            try
            {
                var contentKeys = GetKeys();
                var progression = new StorageAsyncProgression(contentKeys.Length + 1);
                var contentWriter = new InertiaWriter()
                    .SetByte((byte)typeof(T).ToTypeCode())
                    .SetInt(contentKeys.Length);

                for (var i = 0; i < contentKeys.Length; i++)
                {
                    var key = contentKeys[i];
                    var value = this[key];
                    var valueCode = value.GetType().ToTypeCode();

                    contentWriter
                        .SetValue(key)
                        .SetByte((byte)valueCode);

                    if (valueCode == TypeCode.Object)
                    {
                        if (value is Array)
                            SetArray(value, contentWriter);
                        else
                            SetObject(value, contentWriter);
                    }
                    else
                        contentWriter.SetValue(value);

                    if (async)
                    {
                        progression.Current++;
                        SaveProgress(this, progression);
                    }
                }

                if (async)
                {
                    progression.Current++;
                    SaveProgress(this, progression);
                }

                return contentWriter.ExportAndDispose();
            }
            catch (Exception e)
            {
                SaveFailed(this, e);
            }

            return null;
        }
        internal override void Deserialize(byte[] data, bool async)
        {
            Array GetArrayObject(InertiaReader reader)
            {
                var length = reader.GetInt();
                var arrayCode = (ArrayTypeCode)reader.GetByte();
                Array array = null;

                switch (arrayCode)
                {
                    case ArrayTypeCode.BooleanArray:
                        array = new bool[length];
                        break;
                    case ArrayTypeCode.CharArray:
                        array = new char[length];
                        break;
                    case ArrayTypeCode.SByteArray:
                        array = new sbyte[length];
                        break;
                    case ArrayTypeCode.ByteArray:
                        array = new byte[length];
                        break;
                    case ArrayTypeCode.Int16Array:
                        array = new short[length];
                        break;
                    case ArrayTypeCode.UInt16Array:
                        array = new ushort[length];
                        break;
                    case ArrayTypeCode.Int32Array:
                        array = new int[length];
                        break;
                    case ArrayTypeCode.UInt32Array:
                        array = new uint[length];
                        break;
                    case ArrayTypeCode.Int64Array:
                        array = new long[length];
                        break;
                    case ArrayTypeCode.UInt64Array:
                        array = new ulong[length];
                        break;
                    case ArrayTypeCode.SingleArray:
                        array = new float[length];
                        break;
                    case ArrayTypeCode.DoubleArray:
                        array = new double[length];
                        break;
                    case ArrayTypeCode.DecimalArray:
                        array = new decimal[length];
                        break;
                    case ArrayTypeCode.StringArray:
                        array = new string[length];
                        break;
                    case ArrayTypeCode.SerializableArray:
                        array = new ISerializableObject[length];
                        break;
                    case ArrayTypeCode.ObjectArray:
                        array = new object[length];
                        break;
                    default:
                        throw new InvalidSerializableTypeException("Array:" + arrayCode);
                }

                for (var i = 0; i < length; i++)
                {
                    var _vc = (TypeCode)reader.GetByte();
                    if (_vc == TypeCode.Object)
                    {
                        if (reader.GetBool())
                            array.SetValue(GetArrayObject(reader), i);
                        else
                            array.SetValue(GetSerializableObject(reader), i);
                    }
                    else
                        array.SetValue(reader.GetValue(_vc.ToType()), i);
                }

                return array;
            }
            ISerializableObject GetSerializableObject(InertiaReader reader)
            {
                var typeName = reader.GetString();
                var instance = InertiaExtensions.GetStorageInterfaceInstanceFromTypeName(typeName);

                if (instance == null)
                    throw new InvalidSerializableTypeException(typeName);

                var objReader = new InertiaReader(reader.GetBytes());

                instance.Deserialize(objReader);
                objReader.Dispose();

                return instance;
            }

            Container?.Dispose();
            Container = new FlexDictionary<T>();

            try
            {
                var contentReader = new InertiaReader(data);

                var keyCode = (TypeCode)contentReader.GetByte();

                if (keyCode != typeof(T).ToTypeCode())
                    throw new KeyTypeUnmatchException(typeof(T).ToTypeCode(), keyCode);

                var count = contentReader.GetInt();
                var progression = new StorageAsyncProgression(count);

                for (var i = 0; i < count; i++)
                {
                    var key = (T)contentReader.GetValue(typeof(T));
                    var valueCode = (TypeCode)contentReader.GetByte();

                    switch (valueCode)
                    {
                        case TypeCode.Boolean:
                            Container.Add(key, contentReader.GetBool());
                            break;
                        case TypeCode.Char:
                            Container.Add(key, contentReader.GetChar());
                            break;
                        case TypeCode.SByte:
                            Container.Add(key, contentReader.GetSByte());
                            break;
                        case TypeCode.Byte:
                            Container.Add(key, contentReader.GetByte());
                            break;
                        case TypeCode.Int16:
                            Container.Add(key, contentReader.GetShort());
                            break;
                        case TypeCode.UInt16:
                            Container.Add(key, contentReader.GetUShort());
                            break;
                        case TypeCode.Int32:
                            Container.Add(key, contentReader.GetInt());
                            break;
                        case TypeCode.UInt32:
                            Container.Add(key, contentReader.GetUInt());
                            break;
                        case TypeCode.Int64:
                            Container.Add(key, contentReader.GetLong());
                            break;
                        case TypeCode.UInt64:
                            Container.Add(key, contentReader.GetULong());
                            break;
                        case TypeCode.Single:
                            Container.Add(key, contentReader.GetFloat());
                            break;
                        case TypeCode.Double:
                            Container.Add(key, contentReader.GetDouble());
                            break;
                        case TypeCode.Decimal:
                            Container.Add(key, contentReader.GetDecimal());
                            break;
                        case TypeCode.String:
                            Container.Add(key, contentReader.GetString());
                            break;
                        case TypeCode.Object:
                            if (contentReader.GetBool())
                                Container.Add(key, GetArrayObject(contentReader));
                            else
                                Container.Add(key, GetSerializableObject(contentReader));
                            break;
                    }

                    if (async)
                    {
                        progression.Current++;
                        LoadProgress(this, progression);
                    }
                }

                contentReader.Dispose();
            }
            catch (Exception e)
            {
                LoadFailed(this, e);
            }
        }

        internal override void OnDispose()
        {
            Container.Dispose();
            Container = null;
        }

        internal override void OnSaveCompleted()
        {
            Saved(this);
        }
        internal override void OnLoadCompleted()
        {
            Loaded(this);
        }
        internal override void OnSaveFailed(Exception e)
        {
            SaveFailed(this, e);
        }
        internal override void OnLoadFailed(Exception e)
        {
            LoadFailed(this, e);
        }
    }
}

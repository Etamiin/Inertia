using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia
{
    public class DataStorage<T> : DataStorageBase<T>, IDisposable
    {
        public override string Extension => ".is";

        public DataStorage(string name = "") : base(name)
        {
        }

        protected override byte[] Serialize()
        {
            var writer = new Writer();
            var progression = new StorageUpdate(this, 0, Count);

            try
            {
                writer.SetInt(Count);
                foreach (var key in _data.GetKeys())
                {
                    var value = _data.GetDatasExtension<object>(key);
                    var typeCode = value.GetDatasTypeCode();

                    writer
                        .SetValue(key)
                        .SetByte((byte)typeCode);

                    if (typeCode == TypeCode.String)
                        writer.SetString(value.Data.ToString());
                    else if (typeCode == TypeCode.Byte)
                    {
                        writer
                            .SetBool(value.IsByteArray)
                            .SetValue(value.Data);
                    }
                    else
                        writer.SetValue(value.Data);

                    progression.AddOne($"{ key } => { value }");
                    OnSaveProgress(progression);
                }

                var body = writer.ExportAndDispose();
                var isCompressed = Compression.Compress(body, out body);

                var final = new Writer();

                final
                    .SetBool(isCompressed)
                    .SetBytes(body);

                return final.ExportAndDispose();
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

            return null;
        }
        protected override MultiDictionary<T> Deserialize(byte[] data)
        {
            var values = new MultiDictionary<T>();

            try
            {
                var begin = new Reader(data);
                var isCompressed = begin.GetBool();
                var body = isCompressed ? Compression.Decompress(begin.GetBytes()) : begin.GetBytes();

                begin.Dispose();

                var reader = new Reader(body);
                var count = reader.GetInt();
                var progression = new StorageUpdate(this, 0, count);

                for (var i = 0; i < count; i++)
                {
                    var key = reader.GetValue(typeof(T));
                    var code = (TypeCode)reader.GetByte();

                    switch (code)
                    {
                        case TypeCode.Boolean:
                            values.Add((T)key, reader.GetBool());
                            break;
                        case TypeCode.Char:
                            values.Add((T)key, reader.GetChar());
                            break;
                        case TypeCode.SByte:
                            values.Add((T)key, reader.GetSByte());
                            break;
                        case TypeCode.Byte:
                            {
                                bool isArray = reader.GetBool();
                                if (!isArray)
                                    values.Add((T)key, reader.GetByte());
                                else
                                    values.Add((T)key, reader.GetBytes());
                            }
                            break;
                        case TypeCode.Int16:
                            values.Add((T)key, reader.GetShort());
                            break;
                        case TypeCode.UInt16:
                            values.Add((T)key, reader.GetUShort());
                            break;
                        case TypeCode.Int32:
                            values.Add((T)key, reader.GetInt());
                            break;
                        case TypeCode.UInt32:
                            values.Add((T)key, reader.GetUInt());
                            break;
                        case TypeCode.Int64:
                            values.Add((T)key, reader.GetLong());
                            break;
                        case TypeCode.UInt64:
                            values.Add((T)key, reader.GetULong());
                            break;
                        case TypeCode.Single:
                            values.Add((T)key, reader.GetFloat());
                            break;
                        case TypeCode.Double:
                            values.Add((T)key, reader.GetDouble());
                            break;
                        case TypeCode.Decimal:
                            values.Add((T)key, reader.GetDecimal());
                            break;
                        case TypeCode.String:
                            values.Add((T)key, reader.GetString());
                            break;
                        default:
                            values.Add((T)key, reader.GetString());
                            break;
                    }

                    progression.AddOne($"{ key } => { values.Get((T)key).ToString() }");
                    OnLoadProgress(progression);
                }

                reader.Dispose();
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
            return values;
        }

        public void Dispose()
        {
            _data.Dispose();
        }
    }
}

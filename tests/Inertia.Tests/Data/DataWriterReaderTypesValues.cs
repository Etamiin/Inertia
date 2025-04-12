using Inertia.IO;
using System;
using System.Collections;
using System.Text;

namespace Inertia.Tests.Data
{
    public class DataWriterReaderTypesValues
    {
        public bool Bool { get; set; }
        public string? String { get; set; }
        public byte Byte { get; set; }
        public sbyte SByte { get; set; }
        public char Char { get; set; }
        public ByteBits? ByteBits { get; set; }
        public short Short { get; set; }
        public ushort UShort { get; set; }
        public float Float { get; set; }
        public double Double { get; set; }
        public decimal Decimal { get; set; }
        public int Int { get; set; }
        public uint UInt { get; set; }
        public long Long { get; set; }
        public ulong ULong { get; set; }
        public DateTime DateTime { get; set; }
        public byte[]? Bytes { get; set; }
        public ISerializable? ISerializable { get; set; }
        public List<byte>? IEnumerable { get; set; }
        public IDictionary? IDictionary { get; set; }
        public Enum? Enum { get; set; }
        public object? Object { get; set; }
        public ComplexAutoSerializableObject AutoSerializable { get; set; }

        public DataWriterReaderTypesValues()
        {
            Bool = true;
            String = "Hello, World!";
            Byte = 1;
            SByte = -1;
            Char = 'A';
            ByteBits = 2;
            Short = 3;
            UShort = 4;
            Float = 5.5f;
            Double = 6.6;
            Decimal = 7.7m;
            Int = 8;
            UInt = 9;
            Long = 10;
            ULong = 11;
            DateTime = new DateTime(2024, 11, 22, 10, 30, 0);
            Bytes = Encoding.UTF8.GetBytes("Hello, World!");
            ISerializable = new SimpleISerializableObject();
            IEnumerable = new List<byte> { 12, 13, 14, 15 };
            IDictionary = new Dictionary<string, byte>{ { "Key1", 16 }, { "Key2", 17 } };
            Enum = DataEnum.Second;
            Object = new SimpleAutoSerializableObject();
            AutoSerializable = new ComplexAutoSerializableObject();

        }
    }
}

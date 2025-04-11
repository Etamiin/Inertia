using Inertia.IO;
using Inertia.Tests.Data;

namespace Inertia.Tests
{
    public class DataWriterReaderTests
    {
        private const string EncryptionKeyTest = "{ga.kmwlU3^YQDv";
        private static byte[] Salt = new byte[] { 5, 7, 9, 11, 13, 15, 17 };

        [Theory]
        [InlineData(null, CompressionAlgorithm.None)]
        [InlineData(null, CompressionAlgorithm.GZip)]
        [InlineData(null, CompressionAlgorithm.Deflate)]
        [InlineData(EncryptionKeyTest, CompressionAlgorithm.None)]
        [InlineData(EncryptionKeyTest, CompressionAlgorithm.GZip)]
        [InlineData(EncryptionKeyTest, CompressionAlgorithm.Deflate)]
        public void ReadWrite_AllTypes(string encryptionKey, CompressionAlgorithm compressionAlgorithm)
        {
            CompareWriterReaderDataValues(new BinaryTransformationProcessor(encryptionKey, Salt, compressionAlgorithm));
        }

        [Theory]
        [InlineData(null, CompressionAlgorithm.None)]
        [InlineData(EncryptionKeyTest, CompressionAlgorithm.GZip)]
        [InlineData(EncryptionKeyTest, CompressionAlgorithm.Deflate)]
        public void Reader_Fill(string encryptionKey, CompressionAlgorithm compressionAlgorithm)
        {
            var binaryProcessor = new BinaryTransformationProcessor(encryptionKey, Salt, compressionAlgorithm);
            var values = new DataWriterReaderTypesValues();
            var writerData = GetWriterData(values, binaryProcessor);

            using (var reader = new DataReader(writerData, binaryProcessor))
            {
                CompareWriterReaderDataValues(reader);

                reader.RemoveReadedBytes();
                reader.Fill(writerData, binaryProcessor);

                CompareWriterReaderDataValues(reader);
            }
        }

        private void CompareWriterReaderDataValues(BinaryTransformationProcessor binaryProcessor)
        {
            var values = new DataWriterReaderTypesValues();
            var writerData = GetWriterData(values, binaryProcessor);

            using (var reader = new DataReader(writerData, binaryProcessor))
            {
                CompareWriterReaderDataValues(reader);
            }
        }
        private void CompareWriterReaderDataValues(DataReader reader)
        {
            var values = new DataWriterReaderTypesValues();

            Assert.Equal(reader.ReadBool(), values.Bool);
            Assert.Equal(reader.ReadString(), values.String);
            Assert.Equal(reader.ReadByte(), values.Byte);
            Assert.Equal(reader.ReadSByte(), values.SByte);
            Assert.Equal(reader.ReadChar(), values.Char);
            Assert.Equal((byte)reader.ReadByteBits(), (byte)values.ByteBits);
            Assert.Equal(reader.ReadShort(), values.Short);
            Assert.Equal(reader.ReadUShort(), values.UShort);
            Assert.Equal(reader.ReadFloat(), values.Float);
            Assert.Equal(reader.ReadDouble(), values.Double);
            Assert.Equal(reader.ReadDecimal(), values.Decimal);
            Assert.Equal(reader.ReadInt(), values.Int);
            Assert.Equal(reader.ReadUInt(), values.UInt);
            Assert.Equal(reader.ReadLong(), values.Long);
            Assert.Equal(reader.ReadULong(), values.ULong);
            Assert.Equal(reader.ReadDateTime(), values.DateTime);
            Assert.Equal(reader.ReadBytes(values.Bytes.Length), values.Bytes);
            Assert.Equal(reader.ReadBytesWithHeader(), values.Bytes);
            Assert.Equal(reader.ReadSerializable<SimpleISerializableObject>(), values.ISerializable);
            Assert.Equal(reader.ReadIEnumerable<List<byte>>(), values.IEnumerable);
            Assert.Equal(reader.ReadDictionary(values.IDictionary.GetType()), values.IDictionary);
            Assert.Equal(reader.ReadEnum<DataEnum>(), values.Enum);
            Assert.Equal(reader.ReadValue(values.Object.GetType()), values.Object);
            Assert.Equal(reader.ReadValue(values.AutoSerializable.GetType()), values.AutoSerializable);
        }

        private byte[] GetWriterData(DataWriterReaderTypesValues values, BinaryTransformationProcessor binaryProcessor)
        {
            using (var writer = new DataWriter())
            {
                writer
                    .Write(values.Bool)
                    .Write(values.String)
                    .Write(values.Byte)
                    .Write(values.SByte)
                    .Write(values.Char)
                    .Write(values.ByteBits)
                    .Write(values.Short)
                    .Write(values.UShort)
                    .Write(values.Float)
                    .Write(values.Double)
                    .Write(values.Decimal)
                    .Write(values.Int)
                    .Write(values.UInt)
                    .Write(values.Long)
                    .Write(values.ULong)
                    .Write(values.DateTime)
                    .Write(values.Bytes)
                    .WriteWithHeader(values.Bytes)
                    .Write(values.ISerializable)
                    .Write(values.IEnumerable)
                    .Write(values.IDictionary)
                    .Write(values.Enum)
                    .Write(values.Object)
                    .Write(values.AutoSerializable);

                return writer.ToArray(binaryProcessor);
            }
        }
    }
}
namespace Inertia.Tests.Data
{
    public class SimpleISerializableObject : ISerializable
    {
        public byte Version { get; set; }
        public string? Username { get; set; }
        public byte[]? EncryptedPassword { get; set; }

        public SimpleISerializableObject()
        {
            Username = "Inertia";

            var password = "password";
            EncryptedPassword = password.AesEncrypt("_key", new byte[] { 5, 7, 9, 11, 13, 15 });
        }
        public void Serialize(DataWriter writer)
        {
            writer
                .Write(Username)
                .WriteWithHeader(EncryptedPassword)
                .Write("OK");
        }

        public void Deserialize(byte version, DataReader reader)
        {
            Username = reader.ReadString();
            EncryptedPassword = reader.ReadBytesWithHeader();

            var isOk = reader.ReadString() == "OK";
            if (!isOk)
            {
                throw new InvalidDataException();
            }
        }

        public override bool Equals(object? obj)
        {
            if (obj is SimpleISerializableObject other && other != null)
            {
                return Username == other.Username && EncryptedPassword.SequenceEqual(other.EncryptedPassword);
            }

            return false;
        }
    }
}

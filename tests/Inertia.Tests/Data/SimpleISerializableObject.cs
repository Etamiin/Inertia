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
            using (var encryptionResult = password.AesEncrypt("_key"))
            {
                EncryptedPassword = encryptionResult.GetDataOrThrow();
            }
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
            EncryptedPassword = reader.ReadBytes();

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

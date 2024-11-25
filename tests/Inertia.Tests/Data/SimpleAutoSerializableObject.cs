using Inertia.IO;

namespace Inertia.Tests.Data
{
    [AutoSerializable]
    public class SimpleAutoSerializableObject
    {
        public string? Username { get; set; }
        public string? Password { get; set; }

        public SimpleAutoSerializableObject()
        {
            Username = "Inertia";
            Password = "password";
        }

        public override bool Equals(object? obj)
        {
            if (obj is SimpleAutoSerializableObject other && other != null)
            {
                return Username == other.Username && Password == other.Password;
            }

            return false;
        }
    }
}

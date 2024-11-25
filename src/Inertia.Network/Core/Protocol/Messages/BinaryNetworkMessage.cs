using System;

namespace Inertia.Network
{
    public abstract class BinaryNetworkMessage : NetworkMessage
    {
        public byte[] Binary { get; set; }

        public void WriteIn(Action<DataWriter> writeBinary)
        {
            using (var writer = new DataWriter())
            {
                writeBinary?.Invoke(writer);

                Binary = writer.ToArray();
            }
        }
        public void ReadFrom(Action<DataReader> readBinary)
        {
            if (Binary is null || Binary.Length == 0) return;

            using (var reader = new DataReader(Binary))
            {
                readBinary?.Invoke(reader);
            }
        }
    }
}

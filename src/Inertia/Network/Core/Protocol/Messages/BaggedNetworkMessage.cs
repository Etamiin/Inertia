using Inertia.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inertia.Network
{
    public abstract class BaggedNetworkMessage : NetworkMessage
    {
        private DataWriter? _bagWriter;
        private DataReader? _bagReader;

        public DataWriter StartWriting()
        {
            if (_bagWriter == null)
            {
                _bagWriter = new DataWriter();
            }

            return _bagWriter;
        }
        public bool TryOpen(out DataReader? reader)
        {
            reader = _bagReader;
            _bagReader = null;

            return reader != null;
        }

        public override sealed void Serialize(DataWriter writer)
        {
            base.Serialize(writer);

            var hasBag = _bagWriter != null;
            writer.Write(hasBag);

            if (hasBag)
            {
                writer.Write(_bagWriter.ToArray());
            }
        }
        public override sealed void Deserialize(DataReader reader)
        {
            base.Deserialize(reader);

            var hasBag = reader.ReadBool();
            if (hasBag)
            {
                _bagReader = new DataReader(reader.ReadBytes());
            }
        }
    }
}
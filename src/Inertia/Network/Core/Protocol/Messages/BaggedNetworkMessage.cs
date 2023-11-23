using Inertia.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inertia.Network
{
    public abstract class BaggedNetworkMessage : NetworkMessage
    {
        private BasicWriter? _bagWriter;
        private BasicReader? _bagReader;

        public BasicWriter WriteInBag()
        {
            if (_bagWriter == null)
            {
                _bagWriter = new BasicWriter();
            }

            return _bagWriter;
        }
        public bool TryOpenBag(out BasicReader? reader)
        {
            reader = _bagReader;
            _bagReader = null;

            return reader != null;
        }

        public override sealed void Serialize(BasicWriter writer)
        {
            base.Serialize(writer);

            var hasBag = _bagWriter != null;
            writer.SetBool(hasBag);

            if (hasBag)
            {
                writer.SetBytes(_bagWriter.ToArray());
            }
        }
        public override sealed void Deserialize(BasicReader reader)
        {
            base.Deserialize(reader);

            var hasBag = reader.GetBool();
            if (hasBag)
            {
                _bagReader = new BasicReader(new ReaderFilling(reader.GetBytes()));
            }
        }
    }
}
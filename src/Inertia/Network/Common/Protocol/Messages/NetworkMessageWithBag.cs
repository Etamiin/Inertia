using System;

namespace Inertia.Network
{
    public abstract class NetworkMessageWithBag : NetworkMessage
    {
        private BasicWriter _bagWriter;
        private BasicReader _bagReader;

        public virtual void BaseSerialize(BasicWriter writer) { }
        public virtual void BaseDeserialize(BasicReader reader) { }

        public BasicWriter GetBagWriter()
        {
            if (_bagWriter == null)
            {
                _bagWriter = new BasicWriter();
            }

            return _bagWriter;
        }
        public bool TryConsumeBagReader(BasicAction<BasicReader> consumeReader)
        {
            if (_bagReader != null)
            {
                using (_bagReader)
                {
                    consumeReader(_bagReader);
                }

                _bagReader = null;
                return true;
            }

            return false;
        }

        public sealed override void Serialize(BasicWriter writer)
        {
            if (UseAutoSerialization)
            {
                writer.SetAutoSerializable(this);
            }
            else
            {
                BaseSerialize(writer);
            }

            var hasCustomData = _bagWriter != null && _bagWriter.TotalLength > 0;

            writer.SetBool(hasCustomData);
            if (hasCustomData) writer.SetBytes(_bagWriter.ToArray());
        }
        public sealed override void Deserialize(BasicReader reader)
        {
            if (UseAutoSerialization)
            {
                reader.GetAutoSerializable(this);
            }
            else
            {
                BaseDeserialize(reader);
            }

            if (reader.GetBool())
            {
                _bagReader = new BasicReader(reader.GetBytes());
            }
        }
    }
}

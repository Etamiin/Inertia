namespace Inertia.Network
{
    [IgnoreInReflection]
    public abstract class CustomizableNetworkMessage : NetworkMessage
    {
        public sealed override bool UseAutoSerialization => false;

        private BasicWriter _customWriter;
        private BasicReader _customReader;

        public virtual void BaseSerialize(BasicWriter writer) { }
        public virtual void BaseDeserialize(BasicReader reader) { }

        public BasicWriter GetCustomWriter()
        {
            if (_customWriter == null)
            {
                _customWriter = new BasicWriter();
            }

            return _customWriter;
        }
        public bool TryGetCustomReader(out BasicReader reader)
        {
            reader = _customReader;
            return reader != null;
        }

        public sealed override void Serialize(BasicWriter writer)
        {
            BaseSerialize(writer);

            var hasCustomData = _customWriter != null && _customWriter.TotalLength > 0;

            writer.SetBool(hasCustomData);
            if (hasCustomData) writer.SetBytes(_customWriter.ToArray());
        }
        public sealed override void Deserialize(BasicReader reader)
        {
            BaseDeserialize(reader);

            if (reader.GetBool())
            {
                _customReader = new BasicReader(reader.GetBytes());
            }
        }
    }
}

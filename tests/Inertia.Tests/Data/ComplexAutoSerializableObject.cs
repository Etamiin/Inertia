namespace Inertia.Tests.Data
{
    public class ComplexAutoSerializableObject
    {
        public SimpleAutoSerializableObject? Simple { get; set; }
        public int[]? Ints { get; set; }
        public Dictionary<int, string>? Strings { get; set; }
        public List<string>? NullList { get; set; }

        public ComplexAutoSerializableObject()
        {
            Simple = new SimpleAutoSerializableObject();
            Ints = new[] { 1, 3, 5, 7, 9, 11 };
            Strings = new Dictionary<int, string>();

            foreach (var val in Ints)
            {
                Strings.Add(val, val.ToString());
            }
        }

        public override bool Equals(object? obj)
        {
            if (obj is ComplexAutoSerializableObject other && other != null)
            {
                return
                    Simple.Equals(other.Simple) &&
                    Ints.SequenceEqual(other.Ints) &&
                    Strings.SequenceEqual(other.Strings) &&
                    NullList == other.NullList;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                Simple,
                Ints,
                Strings,
                NullList
            );
        }
    }
}

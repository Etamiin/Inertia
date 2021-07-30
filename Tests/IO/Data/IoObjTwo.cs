using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inertia;

namespace InertiaTests.IO
{
    public class IoObjTwo : ISerializableData
    {
        public byte[] FileData { get; set; }
        public List<int> Values { get; set; }

        public IoObjTwo()
        {
            Values = new List<int>();
        }
        public IoObjTwo(byte[] fileData, List<int> values)
        {
            FileData = fileData;
            Values = values;
        }

        public byte[] Serialize()
        {
            using (BasicWriter writer = new BasicWriter())
            {
                writer
                    .SetBytes(FileData)
                    .SetInt(Values.Count);

                writer.SetValues(Values);

                return writer.ToArray();
            }
        }
        public void Deserialize(byte[] data)
        {
            using (BasicReader reader = new BasicReader(data))
            {
                FileData = reader.GetBytes();

                var count = reader.GetInt();
                for (var i = 0; i < count; i++)
                    Values.Add(reader.GetInt());
            }
        }
    }
}

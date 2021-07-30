using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inertia;

namespace InertiaTests.IO
{
    public class IoObjOne : ISerializableObject
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public byte Sex { get; set; }
        public ulong SocialNumber { get; set; }

        public IoObjOne()
        {

        }
        public IoObjOne(string name, int age, byte sex, ulong socialNumber)
        {
            Name = name;
            Age = age;
            Sex = sex;
            SocialNumber = socialNumber;
        }

        public void Serialize(BasicWriter writer)
        {
            writer
                .SetString(Name)
                .SetInt(Age)
                .SetByte(Sex)
                .SetULong(SocialNumber);

        }
        public void Deserialize(BasicReader reader)
        {
            Name = reader.GetString();
            Age = reader.GetInt();
            Sex = reader.GetByte();
            SocialNumber = reader.GetULong();
        }
    }
}

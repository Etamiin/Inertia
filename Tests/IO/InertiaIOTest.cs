using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inertia;

namespace InertiaTests.IO
{
    public class InertiaIOTest
    {
        public InertiaIOTest()
        {
            var randomPath = @""; //set custom folder path here

            //Files will be modified in this test, create custom folder with random files
            //Then remove the return line
            return;

            var files = IOHelper.GetFilesFromDirectory(randomPath, true);

            Console.WriteLine($"File: { files[0] }");

            var stream = new FileStream(files[0], FileMode.Open);

            Console.WriteLine($"SHA: { IOHelper.GetSHA256(stream) }");

            var obj1 = new IoObjOne("Etamiin", 99, 1, 1237978455679984);
            var obj2 = new IoObjTwo(new byte[] { 1, 2, 3, 4, 5 }, new List<int>() { 99, 1, int.MaxValue });

            using (BasicWriter writer = new BasicWriter())
            {
                writer
                    .SetSerializableObject(obj1)
                    .SetSerializableData(obj2);

                IOHelper.AppendAllBytes(files[0], writer.ToArray());
            }

            var data = File.ReadAllBytes(files[0]);

            data = IOHelper.GzipCompress(data, out _);
            data = IOHelper.EncryptWithString(data, "myKey");

            Console.WriteLine($"New SHA: { IOHelper.GetSHA256(data) }");

            File.WriteAllBytes(files[0], data);

            stream.Dispose();
        }
    }
}

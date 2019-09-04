using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia
{
    public class StorageUpdate
    {
        public readonly StorageBase Storage;
        public readonly int Total;
        public int Current { get; private set; }
        public string LastPathAdded { get; private set; }
        public int PercentProgression => (int)((double)Current / Total * 100);

        internal StorageUpdate(StorageBase storage, int current, int total)
        {
            this.Storage = storage;
            this.Current = current;
            this.Total = total;
        }

        internal void AddOne(string path)
        {
            LastPathAdded = path;
            Current++;
        }
    }
}

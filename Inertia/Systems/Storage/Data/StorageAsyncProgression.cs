using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Storage
{
    public class StorageAsyncProgression
    {
        #region Public variables

        public readonly long Total;
        public long Current { get; internal set; }
        public int Percent
        {
            get
            {
                return (int)(Current * 100 / Total);
            }
        }

        #endregion

        #region Constructors

        internal StorageAsyncProgression(long total)
        {
            Total = total;
        }

        #endregion
    }
}

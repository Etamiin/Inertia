using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Storage
{
    /// <summary>
    /// Represent the progression of an event
    /// </summary>
    public class StorageProgressionEventArgs
    {
        #region Public variables

        /// <summary>
        /// Total elements in the progression
        /// </summary>
        public readonly int TotalCount;
        /// <summary>
        /// Total processed elements in the progression
        /// </summary>
        public int ProcessedCount { get; private set; }
        /// <summary>
        /// Percentage of the current progression
        /// </summary>
        public int PercentageProgression { get; private set; }

        #endregion

        #region Constructors

        internal StorageProgressionEventArgs(int totalCount)
        {
            TotalCount = totalCount;
        }

        #endregion

        internal StorageProgressionEventArgs Progress()
        {
            ProcessedCount++;
            PercentageProgression = (int)((float)ProcessedCount / TotalCount * 100f);

            return this;
        }
    }
}

using System.Collections.Generic;

namespace System
{
    public static class InertiaExtensions
    {
        private readonly static Random Randomizer = new Random();

        public static void Shuffle<T>(this IList<T> collection)
        {
            var iStart = 0;
            T savedValue;

            while (iStart < collection.Count - 1)
            {
                int iRand = Randomizer.Next(iStart, collection.Count);
                savedValue = collection[iStart];
                collection[iStart++] = collection[iRand];
                collection[iRand] = savedValue;
            }
        }
        public static void ThrowIfDisposable(this IDisposable disposable, bool isDisposed)
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException(disposable.GetType().Name);
            }
        }
    }
}
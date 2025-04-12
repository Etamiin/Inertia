using System;

namespace Inertia
{
    public static class Check
    {
        public static void ThrowsIfNull(object obj, string parameterName)
        {
            if (obj is null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }
        public static void ThrowsIfDisposable(IDisposable disposable, bool isDisposed)
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException(disposable.GetType().Name);
            }
        }
    }
}

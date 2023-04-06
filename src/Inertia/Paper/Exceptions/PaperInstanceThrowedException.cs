using System;

namespace Inertia.Paper
{
    public class PaperInstanceThrowedException<T> : Exception where T : PaperObject
    {
        public T Instance { get; private set; }
        public bool StopTick { get; set; }
        public bool DisposeResponsibleInstance { get; set; }

        public PaperInstanceThrowedException(T instance, Exception innerException) : base("An error occured during PenSystem process.", innerException)
        {
            Instance = instance;
        }
    }
}

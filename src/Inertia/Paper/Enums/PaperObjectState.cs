namespace Inertia.Paper
{
    internal enum PaperObjectState : byte
    {
        NoState = 0,
        Initialized = 1,
        Disposing = 2,
        Disposed = 3
    }
}

using Inertia.Logging;

namespace Inertia.Paper
{
    public interface IPenSystem
    {
        ILogger Logger { get; }
        float DeltaTime { get; }
        int TickCount { get; }
        int LayerIndex { get; }

        internal void RegisterPaper(PaperObject obj);
    }
}
namespace Inertia.Paper
{
    internal interface IPenSystem
    {
        float DeltaTime { get; }
        int LayerIndex { get; }

        internal void RegisterPaper(PaperObject obj);
        internal void UnregisterPaper(PaperObject obj);
    }
}
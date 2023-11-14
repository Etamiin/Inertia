namespace Inertia.Paper
{
    internal interface IPenSystem
    {
        float DeltaTime { get; }
        int LayerIndex { get; }

        internal void ArchivePaper(PaperObject obj);
        internal void ClearPaper(PaperObject obj);
    }
}
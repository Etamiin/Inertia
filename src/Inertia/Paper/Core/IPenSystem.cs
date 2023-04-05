namespace Inertia.Paper
{
    internal interface IPenSystem
    {
        float DeltaTime { get; }
        int LayerIndex { get; }

        internal void RegisterComponentData(PaperObject obj);
        internal void UnregisterComponentData(PaperObject obj);
    }
}
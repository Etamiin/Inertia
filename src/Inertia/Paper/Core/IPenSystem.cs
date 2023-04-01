namespace Inertia.Scriptable
{
    internal interface IPenSystem
    {
        float DeltaTime { get; }
        int ExecutionLayer { get; }

        internal void RegisterComponentData(PaperObject obj);
        internal void UnregisterComponentData(PaperObject obj);
    }
}
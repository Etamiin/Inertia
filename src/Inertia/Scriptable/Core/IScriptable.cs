namespace Inertia.Scriptable
{
    internal interface IScriptableSystem
    {
        float DeltaTime { get; }
        int ExecutionLayer { get; }

        internal void RegisterComponentData(ScriptableObject obj);
        internal void UnregisterComponentData(ScriptableObject obj);
    }
}
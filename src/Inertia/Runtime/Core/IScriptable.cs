namespace Inertia.Scriptable
{
    internal interface IScriptable
    {
        float DeltaTime { get; }
        int ExecutionLayer { get; }

        internal void RegisterComponentData(ScriptableData componentData);
        internal void UnregisterComponentData(ScriptableData componentData);
    }
}
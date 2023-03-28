namespace Inertia.Plugins
{
    public enum PluginExecutionResult : byte
    {
        NoState = 0,
        FileNotFound = 1,
        AlreadyLoaded = 2,
        Success = 3
    }
}

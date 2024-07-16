namespace Inertia.Paper
{
    public enum PenExecutionLayerType
    {
        /// <summary>
        /// Execute callbacks based on the processor clock
        /// </summary>
        ProcessorClockBased,
        /// <summary>
        /// Execute callbacks based on the Stopwatch ticks
        /// </summary>
        TickBased
    }
}

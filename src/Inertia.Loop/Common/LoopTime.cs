namespace Inertia.Loop
{
    public class LoopTime
    {
        public LoopTime()
        {
        }
        public LoopTime(float time, float deltaTime)
        {
            Time = time;
            DeltaTime = deltaTime;
        }

        public float Time { get; set; }
        public float DeltaTime { get; set; }
        public uint Ticks { get; set; }
    }
}
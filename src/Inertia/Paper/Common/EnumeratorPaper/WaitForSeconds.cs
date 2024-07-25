namespace Inertia.Paper
{
    public class WaitForSeconds : EnumeratorPaperElement
    {
        private readonly float _seconds;
        private float _currentTime;

        public WaitForSeconds(float seconds)
        {
            _seconds = seconds;
        }

        public override void Update(float deltaTime, IPenSystem penSystem)
        {
            _currentTime += deltaTime;
        }

        public override bool IsProcessed()
        {
            return _currentTime >= _seconds;
        }
    }
}

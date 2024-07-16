namespace Inertia.Paper
{
    public class WaitForEndOfTick : EnumeratorPaperElement
    {
        private int _startTick, _currentTick;

        public WaitForEndOfTick()
        {
        }

        public override void Update(float deltaTime, IPenSystem penSystem)
        {
            if (_startTick == 0)
            {
                _startTick = penSystem.TickCount;
            }

            _currentTick = penSystem.TickCount;
        }

        public override bool IsProcessed()
        {
            return _currentTick > _startTick;
        }
    }
}

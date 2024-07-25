using System;

namespace Inertia.Paper
{
    public class WaitForCondition : EnumeratorPaperElement
    {
        private readonly Func<bool> _condition;

        public WaitForCondition(Func<bool> condition)
        {
            _condition = condition;
        }

        public override void Update(float deltaTime, IPenSystem penSystem)
        {
            //Nothing to process for this class
        }

        public override bool IsProcessed()
        {
            return _condition();
        }
    }
}

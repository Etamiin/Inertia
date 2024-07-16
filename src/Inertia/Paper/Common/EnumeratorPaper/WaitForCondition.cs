using System;
using System.Collections.Generic;
using System.Text;

namespace Inertia.Paper
{
    public class WaitForCondition : EnumeratorPaperElement
    {
        private Func<bool> _condition;

        public WaitForCondition(Func<bool> condition)
        {
            _condition = condition;
        }

        public override void Update(float deltaTime, IPenSystem penSystem)
        {
        }

        public override bool IsProcessed()
        {
            return _condition();
        }
    }
}

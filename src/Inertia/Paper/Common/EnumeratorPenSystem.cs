using Inertia.Logging;
using System;

namespace Inertia.Paper
{
    internal sealed class EnumeratorPenSystem : PenSystem<EnumeratorPaper>
    {
        protected override void Tick()
        {
            // Nothing to process for this pen system on tick
        }

        protected override void Process(EnumeratorPaper paper)
        {
            try
            {
                var current = paper.Enumerator.Current;
                while (current == null)
                {
                    if (!paper.Enumerator.MoveNext())
                    {
                        paper.Dispose();
                        return;
                    }

                    current = paper.Enumerator.Current;
                }

                current.Update(DeltaTime, this);

                if (current.IsProcessed() && !paper.Enumerator.MoveNext())
                {
                    paper.Dispose();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, GetType(), nameof(Process));
                paper.Dispose();
            }
        }
    }
}

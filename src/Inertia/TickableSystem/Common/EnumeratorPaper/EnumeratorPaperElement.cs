namespace Inertia.Paper
{
    public abstract class EnumeratorPaperElement
    {
        public abstract void Update(float deltaTime, IPenSystem penSystem);
        public abstract bool IsProcessed();
    }
}

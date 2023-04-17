using System;

namespace Inertia
{
    internal class PenLayerTickingArgs : EventArgs
    {
        internal float DeltaTime { get; private set; }
    
        internal PenLayerTickingArgs(float deltaTime)
        {
            DeltaTime = deltaTime;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Inertia.Runtime.Core
{
    public abstract class ScriptComponentData
    {
        public event BasicAction? Destroyed;

        public ScriptComponentData()
        {
            RuntimeManager.RegisterComponentData(this);
        }

        public void OnDestroy()
        {
            Destroyed?.Invoke();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Inertia.Runtime.Core
{
    public abstract class ScriptComponentData : IDisposable
    {
        public event BasicAction? Destroying;

        public bool IsDisposed { get; private set; }

        public ScriptComponentData()
        {
            var component = RuntimeManager.GetScriptComponent(GetType());
            if (component != null) component.RegisterComponentData(this);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (!IsDisposed && disposing)
            {
                Destroying?.Invoke();
            }
        }
    }
}
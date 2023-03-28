using System;
using System.Threading;

namespace Inertia.Plugins
{
    internal sealed class PluginTrace : IDisposable
    {
        internal bool IsDisposed { get; private set; }
        internal IPlugin Plugin { get; private set; }
        internal CancellationTokenSource? Token { get; private set; }

        internal PluginTrace(IPlugin instance, CancellationTokenSource? executionTaskToken = null)
        {
            Plugin = instance;

            if (executionTaskToken != null)
            {
                Token = executionTaskToken;
            }
        }

        public void Dispose()
        {
            OnDispose(true);
        }

        private void OnDispose(bool disposing)
        {
            if (IsDisposed) return;

            if (disposing)
            {
                Plugin.Stopping();

                if (Token != null) Token.Cancel();

                Plugin = null;
                Token = null;
                IsDisposed = true;
            }
        }
    }
}

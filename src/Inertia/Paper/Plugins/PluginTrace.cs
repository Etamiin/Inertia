using System;
using System.Threading;

namespace Inertia.Plugins
{
    internal sealed class PluginTrace : IDisposable
    {
        internal bool IsDisposed { get; private set; }
        internal CancellationTokenSource? Token { get; private set; }

        private IPlugin _instance;

        internal PluginTrace(IPlugin instance, CancellationTokenSource? executionTaskToken = null)
        {
            _instance = instance;

            if (executionTaskToken != null)
            {
                Token = executionTaskToken;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (IsDisposed) return;

            if (disposing)
            {
                _instance.Stopping();

                if (Token != null) Token.Cancel();

                _instance = null;
                Token = null;
            }

            IsDisposed = true;
        }
    }
}
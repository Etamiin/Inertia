using System;

namespace Inertia.Network
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class NetworkConnectionEntity : IDisposable
    {
        /// <summary>
        /// Returns true if the current instance is disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Sends the specified data through the current connection.
        /// </summary>
        /// <param name="data"></param>
        public abstract void Send(byte[] data);
        /// <summary>
        /// Sends the specified <see cref="NetworkMessage"/> through the current connection.
        /// </summary>
        /// <param name="message"></param>
        public abstract void Send(NetworkMessage message);

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
            }
        }
    }
}

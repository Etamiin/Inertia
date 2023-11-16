using System;

namespace Inertia.Network
{
    [IndirectNetworkEntity]
    public interface INetworkConnectionWrapper
    {
        public object? State { get; set; }

        public void Send(NetworkMessage message);
    }

    public static class NetworkConnectionWrapperExtensions
    {
        public static T GetStateAs<T>(this INetworkConnectionWrapper connection)
        {
            if (connection.State is T tState) return tState;

            return default;
        }
        public static void AsState<T>(this INetworkConnectionWrapper connection, Action<T> action)
        {
            if (connection.State is T tState)
            {
                action(tState);
            }
        }
    }
}
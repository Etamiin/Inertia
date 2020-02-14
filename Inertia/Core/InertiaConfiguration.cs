using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inertia.Network;
using Inertia.Internal;

namespace Inertia
{
    public class InertiaConfiguration
    {
        #region Private constants

        private const int BaseAutoQueueSleepTime = 10;
        private const int BaseAutoQueueDisposeTime = 60000;
        private const int BaseMaxScriptExecutionPerSecond = 500;
        private const int BaseNetworkBufferLength = 8192;
        private static readonly NetworkProcessor BaseNetworkProcessor = new InternalNetworkProcessor(null);

        #endregion

        #region Internal constants

        internal const int MaxUpdaterScriptCount = 350;
        internal const int CriticalScriptCount = 10000;

        #endregion

        #region Public variables

        public static bool IsLogActive { get; private set; } = true;
        public static Encoding BaseEncodage { get; private set; } = Encoding.UTF8;
        public static int AutoQueueSleepTime { get; private set; } = BaseAutoQueueSleepTime;
        public static int AutoQueueDisposeTime { get; private set; } = BaseAutoQueueDisposeTime;
        public static int MaxScriptExecutionPerSecond { get; private set; } = BaseMaxScriptExecutionPerSecond;
        public static int NetworkBufferLength { get; private set; } = BaseNetworkBufferLength;
        public static NetworkProcessor NetworkProcessor { get; private set; } = BaseNetworkProcessor;
        
        #endregion

        public static void SetLogActive(bool active)
        {
            IsLogActive = active;
        }
        public static void SetBaseEncodage(Encoding encodage)
        {
            if (encodage == null)
                encodage = Encoding.UTF8;
            BaseEncodage = encodage;
        }
        public static void SetAutoQueueSleepTime(uint ms)
        {
            AutoQueueSleepTime = (int)ms;
        }
        public static void SetAutoQueueDisposeTime(uint ms)
        {
            AutoQueueDisposeTime = (int)ms;
        }
        public static void SetMaxScriptExecutionPerSecond(uint count)
        {
            count = Math.Min(count, 1000);
            MaxScriptExecutionPerSecond = (int)count;
        }
        public static void SetNetworkBufferLength(uint length)
        {
            NetworkBufferLength = (int)length;
        }
        public static void SetNetworkProcessor<T>() where T : NetworkProcessor
        {
            NetworkProcessor = (NetworkProcessor)typeof(T).GetConstructor(new Type[] { typeof(object) }).Invoke(new object[] { null });
        }
    }
}

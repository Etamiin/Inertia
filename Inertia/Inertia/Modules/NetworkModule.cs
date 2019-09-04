using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Inertia
{
    public class NetworkModule
    {
        private static NetworkModule _instance;
        public static NetworkModule Module
        {
            get
            {
                if (_instance == null)
                    _instance = new NetworkModule();
                return _instance;
            }
        }

        public static int BufferLength = 4096;

        private Dictionary<Type, Message> _messages = new Dictionary<Type, Message>();
        private Dictionary<uint, Type> _messageTypes = new Dictionary<uint, Type>();

        internal NetworkModule()
        {
            LoadPackets();
        }

        private void LoadPackets()
        {
            var Assemblys = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in Assemblys)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (type.IsClass)
                    {
                        if (type.IsSubclassOf(typeof(Message)) && !type.IsAbstract)
                        {
                            var packet = (Message)type.GetConstructor(Type.EmptyTypes).Invoke(new object[] { });
                            if (!AddMessage(packet)) {
                                Logger.Error($"Packet [{ packet.Id }] already exist");
                            }
                        }
                    }
                }
            }
        }
        private bool AddMessage(Message message)
        {
            if (!_messages.ContainsKey(message.GetType())) {
                _messages.Add(message.GetType(), message);
                _messageTypes.Add(message.Id, message.GetType());
                return true;
            }

            return false;
        }

        public Message GetMessage(uint Id)
        {
            _messageTypes.TryGetValue(Id, out Type type);
            if (type != null) {
                _messages.TryGetValue(type, out Message packet);
                return packet;
            }
            return null;
        }
        public Message GetMessage(Type packetType)
        {
            _messages.TryGetValue(packetType, out Message packet);
            return packet;
        }
        public T GetMessage<T>() where T : Message
        {
            _messages.TryGetValue(typeof(T), out Message packet);
            return (T)packet;
        }
    }
}

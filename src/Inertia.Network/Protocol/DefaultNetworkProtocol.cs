using Inertia.IO;
using Inertia.Logging;
using System;

namespace Inertia.Network
{
    internal sealed class DefaultNetworkProtocol : NetworkProtocol
    {
        private const int HeaderSize = sizeof(ushort) + sizeof(uint);

        internal DefaultNetworkProtocol()
        {
        }
        
        public override int NetworkBufferLength => 4096;
        public override int MaxReceivedMessagePerSecondPerClient => 90;

        public override byte[] SerializeMessage(NetworkMessage message)
        {
            using (var writer = new DataWriter())
            {
                writer.Write(message.MessageId);

                var sizePosition = writer.Position;
                writer
                    .SetEmpty(sizeof(uint))
                    .Write(message);

                var size = (uint)(writer.TotalLength - HeaderSize);
                writer.Position = sizePosition;

                writer.Write(size);

                return writer.ToArray();
            }
        }
        public override bool TryDeserializeMessage(NetworkEntity receiver, DataReader reader, MessageParsingOutput output)
        {
            reader.Position = 0;

            try
            {
                long lastFullyReadedPos = 0;

                while (reader.UnreadedLength > 0)
                {
                    var msgId = reader.ReadUShort();
                    var msgSize = reader.ReadUInt();

                    if (reader.UnreadedLength < msgSize) break;

                    if (!NetworkManager.TryGetMessageType(msgId, out var messageType))
                    {
                        reader
                            .Skip((int)msgSize)
                            .RemoveReadedBytes();

                        throw new UnknownMessageException(msgId);
                    }

                    var message = (NetworkMessage)reader.ReadValue(messageType);
                    output.AddMessage(message);

                    lastFullyReadedPos = reader.Position;
                }

                if (lastFullyReadedPos != 0)
                {
                    reader.Position = lastFullyReadedPos;
                    reader.RemoveReadedBytes();
                }

                return true;
            }
            catch (Exception ex)
            {
                LoggingProvider.LogHandler.Log(LogLevel.Error, $"Error while deserializing message on protocol '{GetType().Name}'.", ex);

                receiver.Disconnect(NetworkStopReason.InvalidDataReceived);
                
                return false;
            }            
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia
{
    public abstract class Message
    {
        public abstract uint Id { get; }

        public virtual void OnReceivedFromClient(ClientBase client, Reader reader)
        {
        }
        public virtual void OnReceivedFromServer(UserDatagram.User user, Reader reader)
        {
        }
        public virtual void OnReceivedFromServer(TransmissionControl.User user, Reader reader)
        {
        }

        internal static byte GetHeaderSize()
        {
            return
                sizeof(uint) +
                sizeof(bool) +
                sizeof(int);
                
        }
        internal static bool Read(Reader reader, out Reader contentReader, out Message message)
        {
            contentReader = null;
            message = null;

            if (GetHeaderSize() >= reader.LengthAvailable) {
                Logger.Error("Message reading error >> invalid header size");
                return false;
            }

            var headerReader = new Reader(reader.GetBytes());

            message = NetworkModule.Module.GetMessage(headerReader.GetUInt());
            if (message == null) {
                Logger.Error("Message reading error >> invalid messageId");
                return false;
            }

            var isCompressed = headerReader.GetBool();
            var contentSize = headerReader.GetInt();

            if (reader.LengthAvailable - sizeof(int) < contentSize) {
                Logger.Log("Message reading >> not full data, waiting data...");
                return false;
            }

            contentReader = new Reader(isCompressed ? Compression.Decompress(reader.GetBytes()) : reader.GetBytes());
            headerReader.Dispose();

            return true;
        }
        private static byte[] BuildHeader(Message message, bool isCompressed, int contentSize)
        {
            var headerBuilder = new Writer();

            headerBuilder
                .SetUInt(message.Id)
                .SetBool(isCompressed)
                .SetInt(contentSize);

            return headerBuilder.ExportAndDispose();
        }
        public static byte[] Build(Message message, params object[] values)
        {
            if (message == null)
                return new byte[] { };

            var builderContent = new Writer();
            builderContent.SetValues(values);

            var content = builderContent.ExportAndDispose();
            var header = BuildHeader(message, Compression.Compress(content, out content), content.Length);
            var messageBuilder = new Writer();

            messageBuilder
                .SetBytes(header)
                .SetBytes(content);

            return messageBuilder.ExportAndDispose();
        }
        public static byte[] Build<T>(params object[] values) where T : Message
        {
            var message = NetworkModule.Module.GetMessage<T>();
            return Build(message, values);
        }

    }
}

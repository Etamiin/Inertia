using System.Security.Cryptography.X509Certificates;

namespace Inertia.Network
{
    public sealed class WebSocketServerParameters : ServerParameters
    {
        public readonly X509Certificate SslCertificate;

        public WebSocketServerParameters(int port) : base(port)
        {
        }
        public WebSocketServerParameters(string ip, int port) : base(ip, port)
        {
        }
        public WebSocketServerParameters(int port, X509Certificate sslCertificate) : this(string.Empty, port, sslCertificate)
        {
        }
        public WebSocketServerParameters(string ip, int port, X509Certificate sslCertificate) : base(ip, port)
        {
            SslCertificate = sslCertificate;
        }
    }
}

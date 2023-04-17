using System.Security.Cryptography.X509Certificates;

namespace Inertia.Network
{
    public class WebSocketServerParameters : TcpServerParameters
    {
        public X509Certificate? SslCertificate => _certificate;

        private X509Certificate? _certificate;

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
            _certificate = sslCertificate;
        }
    }
}
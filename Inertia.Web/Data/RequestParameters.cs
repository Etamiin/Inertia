using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Web
{
    public class RequestParameters
    {
        private Dictionary<CoreRequestHeaders, string> m_coreHeaders;
        private Dictionary<string, string> m_headers;
        private long m_contentLength = -1;
        private DateTime m_date;
        private DateTime m_modifiedSince;
        private IWebProxy m_proxy;
        private DecompressionMethods m_decompressionMethod;

        public RequestParameters()
        {
            m_coreHeaders = new Dictionary<CoreRequestHeaders, string>();
            m_headers = new Dictionary<string, string>();
        }

        public string x { get; set; }

        public RequestParameters AddHeader(string name, string value)
        {
            if (m_headers.ContainsKey(name))
                m_headers[name] = value;
            else
                m_headers.Add(name, value);

            return this;
        }
        public RequestParameters SetCoreHeader(CoreRequestHeaders header, string value)
        {
            if (m_coreHeaders.ContainsKey(header))
                m_coreHeaders[header] = value;
            else
                m_coreHeaders.Add(header, value);

            return this;
        }

        public RequestParameters SetAutomaticDecompression(DecompressionMethods decompressionMethods)
        {
            m_decompressionMethod = decompressionMethods;
            return this;
        }
        public RequestParameters SetContentLength(long length)
        {
            m_contentLength = length;
            return this;
        }
        public RequestParameters SetDate(DateTime date)
        {
            m_date = date;
            return this;
        }
        public RequestParameters SetIfModifiedSince(DateTime modifiedSince)
        {
            m_modifiedSince = modifiedSince;
            return this;
        }
        public RequestParameters SetProxy(IWebProxy proxy)
        {
            m_proxy = proxy;
            return this;
        }

        internal void ApplyToRequest(HttpWebRequest request)
        {
            foreach (var coreHeader in m_coreHeaders)
            {
                var property = request.GetType().GetProperty(coreHeader.Key.ToString());
                if (property != null)
                    property.SetValue(request, coreHeader.Value);
            }

            if (m_contentLength != -1)
                request.ContentLength = m_contentLength;
            if (m_date != new DateTime())
                request.Date = m_date;
            if (m_modifiedSince != new DateTime())
                request.IfModifiedSince = m_modifiedSince;
            if (m_proxy != null)
                request.Proxy = m_proxy;
            if (m_decompressionMethod != DecompressionMethods.None)
                request.AutomaticDecompression = m_decompressionMethod;

            foreach (var header in m_headers)
                request.Headers.Add(header.Key + ": " + header.Value);
        }

    }
}

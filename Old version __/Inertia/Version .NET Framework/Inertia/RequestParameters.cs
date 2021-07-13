using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Web
{
    //TODO: RequestParameters docs
    public class RequestParameters
    {
        private Dictionary<CoreRequestHeaders, string> _coreHeaders;
        private Dictionary<string, string> _headers;
        private long _contentLength = -1;
        private DateTime _date;
        private DateTime _modifiedSince;
        private IWebProxy _proxy;
        private DecompressionMethods _decompressionMethod;

        public RequestParameters()
        {
            _coreHeaders = new Dictionary<CoreRequestHeaders, string>();
            _headers = new Dictionary<string, string>();
        }

        public RequestParameters AddHeader(string name, string value)
        {
            if (_headers.ContainsKey(name))
                _headers[name] = value;
            else
                _headers.Add(name, value);

            return this;
        }
        public RequestParameters SetCoreHeader(CoreRequestHeaders header, string value)
        {
            if (_coreHeaders.ContainsKey(header))
                _coreHeaders[header] = value;
            else
                _coreHeaders.Add(header, value);

            return this;
        }

        public RequestParameters SetAutomaticDecompression(DecompressionMethods decompressionMethods)
        {
            _decompressionMethod = decompressionMethods;
            return this;
        }
        public RequestParameters SetContentLength(long length)
        {
            _contentLength = length;
            return this;
        }
        public RequestParameters SetDate(DateTime date)
        {
            _date = date;
            return this;
        }
        public RequestParameters SetIfModifiedSince(DateTime modifiedSince)
        {
            _modifiedSince = modifiedSince;
            return this;
        }
        public RequestParameters SetProxy(IWebProxy proxy)
        {
            _proxy = proxy;
            return this;
        }

        internal void ApplyToRequest(HttpWebRequest request)
        {
            foreach (var coreHeader in _coreHeaders)
            {
                var property = request.GetType().GetProperty(coreHeader.Key.ToString());
                if (property != null)
                    property.SetValue(request, coreHeader.Value);
            }

            if (_contentLength != -1)
                request.ContentLength = _contentLength;
            if (_date != new DateTime())
                request.Date = _date;
            if (_modifiedSince != new DateTime())
                request.IfModifiedSince = _modifiedSince;
            if (_proxy != null)
                request.Proxy = _proxy;
            if (_decompressionMethod != DecompressionMethods.None)
                request.AutomaticDecompression = _decompressionMethod;

            foreach (var header in _headers)
                request.Headers.Add(header.Key + ": " + header.Value);
        }
        internal HttpClientHandler CreateClientHandler()
        {
            var handler = new HttpClientHandler();
            if (_proxy != null)
                handler.Proxy = _proxy;

            if (_decompressionMethod != DecompressionMethods.None)
                handler.AutomaticDecompression = _decompressionMethod;
            
            return handler;
        }
        internal void ApplyToHttpClient(HttpClient client)
        {
            if (_coreHeaders.ContainsKey(CoreRequestHeaders.Accept))
                client.DefaultRequestHeaders.Add("Accept", _coreHeaders[CoreRequestHeaders.Accept]);
            if (_coreHeaders.ContainsKey(CoreRequestHeaders.UserAgent))
                client.DefaultRequestHeaders.Add("User-Agent", _coreHeaders[CoreRequestHeaders.UserAgent]);

            //foreach (var coreHeader in _coreHeaders)
            //{
            //    try
            //    {
            //        var property = client.DefaultRequestHeaders.GetType().GetProperty(coreHeader.Key.ToString());
            //        if (property != null)
            //            property.SetValue(client.DefaultRequestHeaders, coreHeader.Value);
            //    }
            //    catch { }

            //}

            if (_date != new DateTime())
                client.DefaultRequestHeaders.Date = _date;
            if (_modifiedSince != new DateTime())
                client.DefaultRequestHeaders.IfModifiedSince = _modifiedSince;

            foreach (var header in _headers)
                client.DefaultRequestHeaders.Add(header.Key, header.Value);
        }
    }
}

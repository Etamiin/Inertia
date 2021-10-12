using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace Inertia
{
    /// <summary>
    /// Contains the parameters of an HTTP request
    /// </summary>
    public sealed class RequestParameters
    {
        private Dictionary<CoreRequestHeaders, string> _coreHeaders;
        private Dictionary<string, string> _headers;
        private long _contentLength = -1;
        private DateTime _date;
        private DateTime _modifiedSince;
        private IWebProxy _proxy;
        private DecompressionMethods _decompressionMethod;

        /// <summary>
        /// Initialize a new instance of the class <see cref="RequestParameters"/>
        /// </summary>
        public RequestParameters()
        {
            _coreHeaders = new Dictionary<CoreRequestHeaders, string>();
            _headers = new Dictionary<string, string>();
        }

        /// <summary>
        /// Add an HTTP header to the request
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public RequestParameters AddHeader(string name, string value)
        {
            if (_headers.ContainsKey(name))
            {
                _headers[name] = value;
            }
            else
            {
                _headers.Add(name, value);
            }

            return this;
        }
        /// <summary>
        /// Add an HTTP specific header to the request
        /// </summary>
        /// <param name="header"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public RequestParameters SetCoreHeader(CoreRequestHeaders header, string value)
        {
            if (_coreHeaders.ContainsKey(header))
            {
                _coreHeaders[header] = value;
            }
            else
            {
                _coreHeaders.Add(header, value);
            }

            return this;
        }

        /// <summary>
        /// Set the request's <see cref="DecompressionMethods"/>
        /// </summary>
        /// <param name="decompressionMethods"></param>
        /// <returns></returns>
        public RequestParameters SetAutomaticDecompression(DecompressionMethods decompressionMethods)
        {
            _decompressionMethod = decompressionMethods;
            return this;
        }
        /// <summary>
        /// Set the request's content length
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public RequestParameters SetContentLength(long length)
        {
            _contentLength = length;
            return this;
        }
        /// <summary>
        /// Set the request's date
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public RequestParameters SetDate(DateTime date)
        {
            _date = date;
            return this;
        }
        /// <summary>
        /// Set the request's modified since date
        /// </summary>
        /// <param name="modifiedSince"></param>
        /// <returns></returns>
        public RequestParameters SetIfModifiedSince(DateTime modifiedSince)
        {
            _modifiedSince = modifiedSince;
            return this;
        }
        /// <summary>
        /// Set the request's proxy
        /// </summary>
        /// <param name="proxy"></param>
        /// <returns></returns>
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
                {
                    property.SetValue(request, coreHeader.Value);
                }
            }

            if (_contentLength != -1)
            {
                request.ContentLength = _contentLength;
            }
            if (_date != new DateTime())
            {
                request.Date = _date;
            }
            if (_modifiedSince != new DateTime())
            {
                request.IfModifiedSince = _modifiedSince;
            }
            if (_proxy != null)
            {
                request.Proxy = _proxy;
            }
            if (_decompressionMethod != DecompressionMethods.None)
            {
                request.AutomaticDecompression = _decompressionMethod;
            }

            foreach (var header in _headers)
            {
                request.Headers.Add(header.Key + ": " + header.Value);
            }
        }
    }
}

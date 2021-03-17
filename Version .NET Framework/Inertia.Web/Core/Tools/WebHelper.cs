using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;

namespace Inertia.Web
{
    /// <summary>
    ///
    /// </summary>
    public static class WebHelper
    {
        /// <summary>
        /// Execute a HTTP GET request with specified parameters and return the string response
        /// </summary>
        /// <param name="uriRequest">Uri to request</param>
        /// <param name="parameters">Parameters to apply to the request</param>
        /// <returns></returns>
        public static string GetRequest(Uri uriRequest, RequestParameters parameters = null)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(uriRequest);
                if (parameters != null)
                    parameters.ApplyToRequest(request);

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                    return reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                ex.GetLogger().Log("GetRequest exception:: " + ex);
                return string.Empty;
            }
        }
        /// <summary>
        /// Execute a HTTP GET asynchronously request
        /// </summary>
        /// <param name="uriRequest">Uri to request</param>
        /// <param name="callback">Action to execute when receiving response</param>
        /// <param name="parameters">Parameters to apply to the request</param>
        /// <returns></returns>
        public static void GetRequestAsync(Uri uriRequest, BasicAction<string> callback, RequestParameters parameters = null)
        {
            Task.Factory.StartNew(() => callback(GetRequest(uriRequest, parameters)));
        }
        /// <summary>
        /// Execute a HTTP GET request and return the byte[] data response
        /// </summary>
        /// <param name="uriRequest">Uri to request</param>
        /// <param name="parameters">Parameters to apply to the request</param>
        /// <returns></returns>
        public static byte[] GetRequestData(Uri uriRequest, RequestParameters parameters = null)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(uriRequest);
                if (parameters != null)
                    parameters.ApplyToRequest(request);

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                {
                    var ms = new MemoryStream();
                    stream.CopyTo(ms);

                    return ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                ex.GetLogger().Log("GetRequestData exception:: " + ex);
                return new byte[] { };
            }
        }
        /// <summary>
        /// Execute a HTTP GET asynchronously request
        /// </summary>
        /// <param name="uriRequest">Uri to request</param>
        /// <param name="callback">Action to execute when receiving response</param>
        /// <param name="parameters">Parameters to apply to the request</param>
        /// <returns></returns>
        public static void GetRequestDataAsync(Uri uriRequest, BasicAction<byte[]> callback, RequestParameters parameters = null)
        {
            Task.Factory.StartNew(() => callback(GetRequestData(uriRequest)));
        }

        /// <summary>
        /// Execute a HTTP POST request and return the string response
        /// </summary>
        /// <param name="uriRequest">Uri to request</param>
        /// <param name="parameters">Parameters to apply to the request</param>
        /// <returns></returns>
        public static string PostRequest(Uri uriRequest, RequestParameters parameters = null)
        {
            return PostRequest(uriRequest, data: "", parameters);
        }
        /// <summary>
        /// Execute a HTTP POST request and return the string response
        /// </summary>
        /// <param name="uriRequest">Uri to request</param>
        /// <param name="data">Data to post</param>
        /// <param name="parameters">Parameters to apply to the request</param>
        /// <returns></returns>
        public static string PostRequest(Uri uriRequest, string data, RequestParameters parameters = null)
        {
            try
            {
                var body = Encoding.ASCII.GetBytes(data);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uriRequest);
                request.Method = "POST";

                if (!string.IsNullOrEmpty(data))
                {
                    parameters.SetContentLength(body.LongLength);

                    using (var requestBody = request.GetRequestStream())
                        requestBody.Write(body, 0, body.Length);
                }

                if (parameters != null)
                    parameters.ApplyToRequest(request);

                using (var response = (HttpWebResponse)request.GetResponse())
                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                ex.GetLogger().Log("PostRequest exception:: " + ex);
                return string.Empty;
            }
        }
        /// <summary>
        /// Execute a HTTP POST request and return the byte[] data response
        /// </summary>
        /// <param name="uriRequest">Uri to request</param>
        /// <param name="data">Data to post</param>
        /// <param name="parameters">Parameters to apply to the request</param>
        /// <returns></returns>
        public static byte[] PostRequestData(Uri uriRequest, string data, RequestParameters parameters = null)
        {
            try
            {
                var dataBytes = Encoding.UTF8.GetBytes(data);

                var request = (HttpWebRequest)WebRequest.Create(uriRequest);
                request.Method = "POST";
                request.ContentLength = dataBytes.Length;

                if (parameters != null)
                    parameters.ApplyToRequest(request);

                using (Stream requestBody = request.GetRequestStream())
                    requestBody.Write(dataBytes, 0, dataBytes.Length);

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                {
                    var ms = new MemoryStream();
                    stream.CopyTo(ms);

                    return ms.ToArray();
                }
            }
            catch (WebException ex)
            {
                if (ex.Response != null && ex.Response.ResponseUri != uriRequest)
                {
                    return PostRequestData(ex.Response.ResponseUri, data, parameters);
                }
                else
                {
                    ex.GetLogger().Log("PostRequestData exception:: " + ex);
                    return new byte[] { };
                }
            }
        }
        /// <summary>
        /// Execute a HTTP POST asynchronously request
        /// </summary>
        /// <param name="uriRequest">Uri to request</param>
        /// <param name="data">Data to post</param>
        /// <param name="callback">Action to execute when receiving response</param>
        /// <param name="parameters">Parameters to apply to the request</param>
        /// <returns></returns>
        public static void PostRequestAsync(Uri uriRequest, string data, BasicAction<string> callback, RequestParameters parameters = null)
        {
            Task.Factory.StartNew(() => callback(PostRequest(uriRequest, data, parameters)));
        }
        /// <summary>
        /// Execute a HTTP POST asynchronously request
        /// </summary>
        /// <param name="uriRequest">Uri to request</param>
        /// <param name="data">Data to post</param>
        /// <param name="callback">Action to execute when receiving response</param>
        /// <param name="parameters">Parameters to apply to the request</param>
        /// <returns></returns>
        public static void PostRequestDataAsync(Uri uriRequest, string data, BasicAction<byte[]> callback, RequestParameters parameters = null)
        {
            Task.Factory.StartNew(() => callback(PostRequestData(uriRequest, data, parameters)));
        }
    }
}

using System;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;

namespace Inertia
{
    /// <summary>
    ///
    /// </summary>
    public static class WebHelper
    {
        /// <summary>
        /// Execute a HTTP GET request with specified parameters and return the string response.
        /// </summary>
        /// <param name="uriRequest"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static string GetRequest(Uri uriRequest, RequestParameters parameters = null)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(uriRequest);
                if (parameters != null)
                {
                    parameters.ApplyToRequest(request);
                }

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                return $"Request Error: { ex.Message }";
            }
        }
        /// <summary>
        /// Execute a HTTP GET request asynchronously.
        /// </summary>
        /// <param name="uriRequest"></param>
        /// <param name="callback"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static void GetRequestAsync(Uri uriRequest, BasicAction<string> callback, RequestParameters parameters = null)
        {
            Task.Factory.StartNew(() => callback?.Invoke(GetRequest(uriRequest, parameters)));
        }
        /// <summary>
        /// Execute a HTTP GET request and return the data response.
        /// </summary>
        /// <param name="uriRequest"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static byte[] GetRequestData(Uri uriRequest, RequestParameters parameters = null)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(uriRequest);
                if (parameters != null)
                {
                    parameters.ApplyToRequest(request);
                }

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
                //redirect
                if (ex.Response != null && ex.Response.ResponseUri != uriRequest)
                {
                    return GetRequestData(ex.Response.ResponseUri, parameters);
                }
            }

            return new byte[0];
        }
        /// <summary>
        /// Execute a HTTP GET request asynchronously.
        /// </summary>
        /// <param name="uriRequest"></param>
        /// <param name="callback"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static void GetRequestDataAsync(Uri uriRequest, BasicAction<byte[]> callback, RequestParameters parameters = null)
        {
            Task.Factory.StartNew(() => callback?.Invoke(GetRequestData(uriRequest)));
        }

        /// <summary>
        /// Execute a HTTP POST request with specified parameters and return the string response.
        /// </summary>
        /// <param name="uriRequest"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static string PostRequest(Uri uriRequest, RequestParameters parameters = null)
        {
            return PostRequest(uriRequest, data: "", parameters);
        }
        /// <summary>
        /// Execute a HTTP POST request with specified parameters and return the string response.
        /// </summary>
        /// <param name="uriRequest"></param>
        /// <param name="data"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static string PostRequest(Uri uriRequest, string data, RequestParameters parameters = null)
        {
            try
            {
                var body = Encoding.ASCII.GetBytes(data);
                var request = (HttpWebRequest)WebRequest.Create(uriRequest);
                request.Method = "POST";

                if (parameters == null)
                {
                    parameters = new RequestParameters();
                }

                if (!string.IsNullOrEmpty(data))
                {
                    parameters.SetContentLength(body.LongLength);

                    using (var requestBody = request.GetRequestStream())
                    {
                        requestBody.Write(body, 0, body.Length);
                    }
                }

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
                return $"Request Error: { ex.Message }";
            }
        }
        /// <summary>
        /// Execute a HTTP POST request and return the data response.
        /// </summary>
        /// <param name="uriRequest"></param>
        /// <param name="data"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static byte[] PostRequestData(Uri uriRequest, string data, RequestParameters parameters = null)
        {
            try
            {
                var dataBytes = Encoding.UTF8.GetBytes(data);
                var request = (HttpWebRequest)WebRequest.Create(uriRequest);
                request.Method = "POST";

                if (parameters == null)
                {
                    parameters = new RequestParameters();
                    parameters.SetContentLength(dataBytes.Length);
                }

                parameters.ApplyToRequest(request);

                using (Stream requestBody = request.GetRequestStream())
                {
                    requestBody.Write(dataBytes, 0, dataBytes.Length);
                }

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
                //redirect
                if (ex.Response != null && ex.Response.ResponseUri != uriRequest)
                {
                    return PostRequestData(ex.Response.ResponseUri, data, parameters);
                }
            }

            return new byte[0];
        }
        /// <summary>
        /// Execute a HTTP POST request asynchronously.
        /// </summary>
        /// <param name="uriRequest"></param>
        /// <param name="data"></param>
        /// <param name="callback"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static void PostRequestAsync(Uri uriRequest, string data, BasicAction<string> callback, RequestParameters parameters = null)
        {
            Task.Factory.StartNew(() => callback?.Invoke(PostRequest(uriRequest, data, parameters)));
        }
        /// <summary>
        /// Execute a HTTP POST request asynchronously.
        /// </summary>
        /// <param name="uriRequest"></param>
        /// <param name="data"></param>
        /// <param name="callback"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static void PostRequestDataAsync(Uri uriRequest, string data, BasicAction<byte[]> callback, RequestParameters parameters = null)
        {
            Task.Factory.StartNew(() => callback?.Invoke(PostRequestData(uriRequest, data, parameters)));
        }
    }
}

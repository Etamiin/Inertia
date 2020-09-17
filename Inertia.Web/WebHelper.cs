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
    /// Offers multiple utilities functions for web networking
    /// </summary>
    public static class WebHelper
    {
        /// <summary>
        /// Execute a HTTP GET request and return the string response
        /// </summary>
        /// <param name="uriRequest">Uri to request</param>
        /// <returns></returns>
        public static string GetRequest(Uri uriRequest)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(uriRequest);
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                    return reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                BaseLogger.DefaultLogger.Log("GetRequest exception:: " + ex);
                return string.Empty;
            }
        }
        /// <summary>
        /// Execute a HTTP GET request and return the byte[] data response
        /// </summary>
        /// <param name="uriRequest">Uri to request</param>
        /// <returns></returns>
        public static byte[] GetRequestData(Uri uriRequest)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(uriRequest);
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
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
                BaseLogger.DefaultLogger.Log("GetRequestData exception:: " + ex);
                return new byte[] { };
            }
        }
        /// <summary>
        /// Execute a HTTP GET asynchronously request
        /// </summary>
        /// <param name="uriRequest">Uri to request</param>
        /// <param name="callback">Action to execute when receiving response</param>
        /// <returns></returns>
        public static void GetRequestAsync(Uri uriRequest, BasicAction<string> callback)
        {
            Task.Factory.StartNew(() => callback(GetRequest(uriRequest)));
        }
        /// <summary>
        /// Execute a HTTP GET asynchronously request
        /// </summary>
        /// <param name="uriRequest">Uri to request</param>
        /// <param name="callback">Action to execute when receiving response</param>
        /// <returns></returns>
        public static void GetRequestDataAsync(Uri uriRequest, BasicAction<byte[]> callback)
        {
            Task.Factory.StartNew(() => callback(GetRequestData(uriRequest)));
        }

        /// <summary>
        /// Execute a HTTP POST request and return the string response
        /// </summary>
        /// <param name="uriRequest">Uri to request</param>
        /// <param name="data">Data to post</param>
        /// <param name="contentType">Content-Type of the request</param>
        /// <returns></returns>
        public static string PostRequest(Uri uriRequest, string data, string contentType = "text/html")
        {
            try
            {
                var body = Encoding.ASCII.GetBytes(data);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uriRequest);
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                request.ContentLength = body.Length;
                request.ContentType = contentType;
                request.Method = "POST";

                using (var requestBody = request.GetRequestStream())
                {
                    requestBody.Write(body, 0, body.Length);
                }
                using (var response = (HttpWebResponse)request.GetResponse())
                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                BaseLogger.DefaultLogger.Log("PostRequest exception:: " + ex);
                return string.Empty;
            }
        }
        /// <summary>
        /// Execute a HTTP POST request and return the byte[] data response
        /// </summary>
        /// <param name="uriRequest">Uri to request</param>
        /// <param name="data">Data to post</param>
        /// <param name="contentType">Content-Type of the request</param>
        /// <returns></returns>
        public static byte[] PostRequestData(Uri uriRequest, string data, string contentType = "text/html")
        {
            try
            {
                byte[] dataBytes = Encoding.UTF8.GetBytes(data);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uriRequest);
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                request.ContentLength = dataBytes.Length;
                request.ContentType = contentType;
                request.Method = "POST";

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
            catch (Exception ex)
            {
                BaseLogger.DefaultLogger.Log("PostRequestData exception:: " + ex);
                return new byte[] { };
            }
        }
        /// <summary>
        /// Execute a HTTP POST asynchronously request
        /// </summary>
        /// <param name="uriRequest">Uri to request</param>
        /// <param name="data">Data to post</param>
        /// <param name="callback">Action to execute when receiving response</param>
        /// <param name="contentType">Content-Type of the request</param>
        /// <returns></returns>
        public static void PostRequestAsync(Uri uriRequest, string data, BasicAction<string> callback, string contentType = "text/html")
        {
            Task.Factory.StartNew(() => callback(PostRequest(uriRequest, data, contentType)));
        }
        /// <summary>
        /// Execute a HTTP POST asynchronously request
        /// </summary>
        /// <param name="uriRequest">Uri to request</param>
        /// <param name="data">Data to post</param>
        /// <param name="callback">Action to execute when receiving response</param>
        /// <param name="contentType">Content-Type of the request</param>
        /// <returns></returns>
        public static void PostRequestDataAsync(Uri uriRequest, string data, BasicAction<byte[]> callback, string contentType = "text/html")
        {
            Task.Factory.StartNew(() => callback(PostRequestData(uriRequest, data, contentType)));
        }

    }
}

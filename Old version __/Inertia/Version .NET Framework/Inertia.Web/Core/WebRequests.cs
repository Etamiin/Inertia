using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Inertia.Web
{
    public class WebRequests
    {
        private RequestParameters _parameters;
        private List<Uri> _getRequests;
        private Dictionary<Uri, HttpContent> _postRequests;
        private int _timeout;
        private bool _isBusy;

        public WebRequests(int timeout = 15000)
        {
            _getRequests = new List<Uri>();
            _postRequests = new Dictionary<Uri, HttpContent>();
            _timeout = timeout;
        }
        public WebRequests(RequestParameters parameters, int timeout = 15000) : this(timeout)
        {
            _parameters = parameters;
        }

        public WebRequests AddGetRequest(Uri uri)
        {
            if (!_isBusy)
                _getRequests.Add(uri);

            return this;
        }
        public WebRequests AddPostRequest(Uri uri, string data)
        {
            if (!_isBusy && !_postRequests.ContainsKey(uri))
            {
                var content = new StringContent(data, Encoding.UTF8, "application/x-www-form-urlencoded");
                _postRequests.Add(uri, content);
            }

            return this;
        }
        public WebRequests AddPostRequest(Uri uri, Dictionary<string, string> data)
        {
            if (!_isBusy && !_postRequests.ContainsKey(uri))
            {
                var sData = string.Empty;

                for (var i = 0; i < data.Count; i++)
                {
                    if (i > 0)
                        sData += "&";

                    var pair = data.ElementAt(i);
                    sData += pair.Key + "=" + pair.Value;
                }

                var esData = HttpUtility.UrlEncode(sData, Encoding.UTF8);
                var bData = Encoding.UTF8.GetBytes(esData);
                var c = new ByteArrayContent(bData);
                c.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

                _postRequests.Add(uri, c);
                //_postRequests.Add(uri, new FormUrlEncodedContent(data));
            }

            return this;
        }

        public void ExecuteAsync(BasicAction<HttpResponseMessage> onGetResponse, BasicAction<HttpResponseMessage> onPostResponse)
        {
            if (_isBusy)
                return;

            var handler = _parameters?.CreateClientHandler();
            var getCount = 0;
            var postCount = 0;

            handler.AllowAutoRedirect = false;

            var client = handler == null ? new HttpClient() : new HttpClient(handler);

            client.Timeout = new TimeSpan(0, 0, 0, 0, _timeout);
            client.DefaultRequestHeaders.Clear();
            
            _parameters?.ApplyToHttpClient(client);

            foreach (var uri in _getRequests)
            {
                Task.Run(async () => {
                    try
                    {
                        var task = client.GetAsync(uri);
                        await task;

                        onGetResponse(task.Result);
                        getCount++;
                    }
                    catch { getCount++; }
                });
            }

            foreach (var pair in _postRequests)
            {
                Task.Run(async () => {
                    try
                    {
                        Console.WriteLine("Posting data...");

                        var request = new HttpRequestMessage(HttpMethod.Post, pair.Key);
                        request.Content = pair.Value;

                        onPostResponse(await client.SendAsync(request));
                        postCount++;
                    }
                    catch (Exception ex) { postCount++; Console.WriteLine(ex); }
                });
            }

            Task.Factory.StartNew(() => {
                while (true)
                {
                    if (_getRequests.Count == getCount && _postRequests.Count == postCount)
                    {
                        _getRequests.Clear();
                        _postRequests.Clear();
                        _isBusy = false;
                        client.Dispose();

                        break;
                    }

                    Task.Delay(1000);
                }
            });
        }
    }
}

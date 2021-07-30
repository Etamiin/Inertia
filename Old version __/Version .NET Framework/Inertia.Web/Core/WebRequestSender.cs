using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Web
{
    public class WebRequestSender
    {
        public readonly HttpClient Client;
        
        public WebRequestSender(int timeout = 20000, RequestParameters parameters = null)
        {
            var handler = parameters?.CreateClientHandler();
            handler.UseCookies = false;
            
            Client = new HttpClient(handler);

            Client.Timeout = new TimeSpan(0, 0, 0, 0, timeout);
            RefreshParameters(parameters);
        }

        public void RefreshParameters(RequestParameters parameters)
        {
            Client.DefaultRequestHeaders.Clear();
            Client.DefaultRequestHeaders.Add("Connection", "keep-alive");
            
            parameters?.ApplyToHttpClient(Client);
        }

        public async Task<HttpResponseMessage> GetRequest(Uri uri, RequestParameters parameters = null)
        {
            if (parameters != null)
                RefreshParameters(parameters);

            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Add("Cookie", "_ga=GA1.2.1331796825.1615580439; _gid=GA1.2.1598740308.1617195309; cookieconsent_status=dismiss; adscount=3; parasubmitvote1=vote");

            return await Client.SendAsync(request);
        }
        public async Task<HttpResponseMessage> PostRequest(Uri uri, Dictionary<string, string> data, RequestParameters parameters = null)
        {
            if (parameters != null)
                RefreshParameters(parameters);

            var request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Content = new FormUrlEncodedContent(data);
            request.Headers.Add("Cookie", "_ga=GA1.2.1331796825.1615580439; _gid=GA1.2.1598740308.1617195309; cookieconsent_status=dismiss; adscount=3; parasubmitvote1=vote");

            BaseLogger.Instance.LogP("OK", "Executing post request ({0})", uri);
            return await Client.SendAsync(request);
        }

    }
}

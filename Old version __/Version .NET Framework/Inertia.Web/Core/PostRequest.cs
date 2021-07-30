using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Web
{
    public class PostRequest
    {

        public PostRequest(Uri uri, Dictionary<string, string> data, BasicAction<HttpResponseMessage> onResponse, int timeout = 20000, RequestParameters parameters = null)
        {
            var handler = parameters?.CreateClientHandler();
            handler.AllowAutoRedirect = false;

            var client = handler == null ? new HttpClient() : new HttpClient(handler);

            client.Timeout = new TimeSpan(0, 0, 0, 0, timeout);
            client.DefaultRequestHeaders.Clear();

            parameters?.ApplyToHttpClient(client);

            var request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Content = new FormUrlEncodedContent(data);

            BaseLogger.Instance.LogP("OK", "Executing post request ({0})", uri);
            var sendTask = client.SendAsync(request);
            sendTask.Wait();

            var response =sendTask.GetAwaiter().GetResult();
            onResponse(response);
        }

    }
}

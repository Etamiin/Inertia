using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Web
{
    public class WebManager
    {
        #region GetInstance

        public static WebManager GetInstance()
        {
            if (Instance == null)
                Instance = new WebManager();

            return Instance;
        }
        private static WebManager Instance;

        #endregion

        #region Events

        public event LoggerHandler HandleError = (error) => { };

        #endregion

        #region Constructors

        internal WebManager()
        {
        }

        #endregion

        public static string ExecuteHttpGet(string uri)
        {
            var response = (HttpWebResponse)WebRequest.Create(uri).GetResponse();
            var dataStream = response.GetResponseStream();
            var reader = new StreamReader(dataStream);

            var content = reader.ReadToEnd();

            reader.Close();
            dataStream.Close();
            response.Close();

            return content;
        }
        public static void ExecuteHttpGetAsync(string uri, InertiaAction<string> onReceivedCallback)
        {
            Task.Factory.StartNew(() => onReceivedCallback(ExecuteHttpGet(uri)));
        }

        public static byte[] DownloadData(string uri)
        {
            using (var client = new WebClient())
            {
                try
                {
                    return client.DownloadData(uri);
                }
                catch (Exception ex)
                {
                    GetInstance().HandleError("Downloading data failed: " + ex.ToString());
                    return null;
                }
            }

        }
        public static void DownloadDataAsync(string uri, InertiaAction<byte[]> onDownloadedCallback)
        {
            Task.Factory.StartNew(() => onDownloadedCallback(DownloadData(uri)));
        }
        public static void DownloadFile(string uri, string destinationPath)
        {
            var data = DownloadData(uri);
            File.WriteAllBytes(destinationPath, data);
        }
        public void DownloadFileAsync(string uri, string destinationPath, InertiaAction onDownloadedCallback)
        {
            Task.Factory.StartNew(() => {
                DownloadFile(uri, destinationPath);
                onDownloadedCallback();                
            });
        }

        public static bool CreateDirectoryOnFtp(string completeFolderUri, FtpCredential credential)
        {
            var folders = completeFolderUri.Split('/');
            var path = string.Empty;
            var created = true;

            foreach (var folder in folders)
            {
                if (string.IsNullOrEmpty(folder))
                    continue;

                path += folder;

                try
                {
                    var request = (FtpWebRequest)WebRequest.Create(new Uri(credential.CompleteHost + path));
                    request.Method = WebRequestMethods.Ftp.MakeDirectory;
                    request.Credentials = (NetworkCredential)credential;
                    request.UsePassive = true;
                    request.UseBinary = true;
                    request.KeepAlive = false;

                    ((FtpWebResponse)request.GetResponse()).Close();
                }
                catch (WebException ex)
                {
                    var response = (FtpWebResponse)ex.Response;
                    var error550 = response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable;

                    response.Close();
                    if (!error550)
                    {
                        created = false;
                        break;
                    }
                }

                path += "/";
            }

            return created;
        }
    }
}

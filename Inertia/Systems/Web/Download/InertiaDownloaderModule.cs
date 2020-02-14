using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Inertia.Internal;

namespace Inertia.Web
{
    public abstract class InertiaDownloaderModule : IDisposable
    {
        #region Events

        public event InertiaAction QueueDownloaded = () => { };
        public event DownloadStartedHandler DownloadFileStarted = (file) => { };
        public event DownloadProgressHandler DownloadFileProgress = (file) => { };
        public event DownloadCompletedHandler DownloadFileCompleted = (file) => { };
        public event DownloadFailedHandler DownloadFileFailed = (file, exception) => { };

        #endregion

        #region Public variables

        public bool IsWorking
        {
            get
            {
                return Client.IsBusy;
            }
        }
        public WebModuleConfiguration WebConfig { get; set; }

        #endregion

        #region Private variables

        private readonly WebClient Client;
        private readonly List<WebFileData> Files;
        private int CurrentAttempt;
        
        #endregion

        #region Constructors

        public InertiaDownloaderModule(string baseUri)
        {
            if (string.IsNullOrEmpty(baseUri))
                throw new NullReferenceException();

            WebConfig = new WebModuleConfiguration();
            WebConfig = new WebModuleConfiguration();
            Client = new WebClient();
            Files = new List<WebFileData>();

            Client.DownloadProgressChanged += Client_DownloadProgressChanged;
            Client.DownloadFileCompleted += Client_DownloadFileCompleted;

            StringConventionNormalizer.NormalizeHostAdressUri(ref baseUri);
            Client.BaseAddress = baseUri;
        }
        public InertiaDownloaderModule(FtpCredential credentials) : this(credentials.CompleteHost)
        {
            if (string.IsNullOrEmpty(credentials.Host))
                throw new NullReferenceException();

            Client.Credentials = (NetworkCredential)credentials;
        }

        #endregion

        public void Enqueue(string uri, string folderDestinationPath)
        {
            if (IsWorking)
                throw new WebModuleBusyException();

            if (string.IsNullOrEmpty(uri) || string.IsNullOrEmpty(folderDestinationPath))
                throw new NullReferenceException();

            if (string.IsNullOrEmpty(uri) || string.IsNullOrEmpty(folderDestinationPath))
                throw new EnqueueInvalidInformationsException(uri);

            if (this is FtpDownloader)
                Files.Add(new WebFileData(((FtpDownloader)this).Credentials.Host, uri, folderDestinationPath));
            else
                Files.Add(new WebFileData(Client.BaseAddress, uri, folderDestinationPath));
        }
        public void EnqueueRange(string[] uris, string folderDestinationPath)
        {
            foreach (var uri in uris)
                Enqueue(uri, folderDestinationPath);
        }

        public void DownloadQueue()
        {
            if (IsWorking)
                throw new WebModuleBusyException();

            if (Files.Count == 0)
                return;

            DownloadNext();
        }
        public void DownloadQueueAsync()
        {
            Task.Factory.StartNew(() => DownloadQueue());
        }

        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            var file = Files[0];
            file.Progress(e);

            DownloadFileProgress(file);
        }
        private void Client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            var file = Files[0];

            if (e.Cancelled || e.Error != null)
            {
                if (++CurrentAttempt == WebConfig.MaxRetryAttempt)
                {
                    DownloadFileFailed(file, e.Error ?? new Exception("Download cancelled file(" + file.Name + ") - Attempts exceeded"));
                    if (WebConfig.StopOnFail)
                        return;
                }
                else {
                    DownloadNext();
                    return;
                }
            }

            DownloadFileCompleted(file);

            CurrentAttempt = 0;
            Files.RemoveAt(0);

            if (Files.Count == 0)
                QueueDownloaded();
            else
                DownloadNext();
        }

        private void DownloadNext()
        {
            var file = Files[0];

            DownloadFileStarted(file);

            if (!Directory.Exists(file.Destination))
                Directory.CreateDirectory(file.Destination);

            Client.DownloadFileAsync(new Uri(Client.BaseAddress + file.CompleteUri), file.Destination + file.Name);
        }

        public void Dispose()
        {
            Client.Dispose();
            WebConfig.Dispose();
            WebConfig = null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Inertia;

namespace Inertia.Web
{
    //TODO: Rework class rematching FtpDownloader
    /// <summary>
    /// 
    /// </summary>
    public class WebDownloader : IDisposable
    {
        /// <summary>
        /// Occurs when the current queue is completely downloaded
        /// </summary>
        public event BasicAction QueueDownloaded = () => { };
        /// <summary>
        /// Occurs when the current queue downloading progress
        /// </summary>
        public event BasicAction<int, int> QueueDownloadProgress = (current, total) => { };
        /// <summary>
        /// Occurs when a file start to be downloaded
        /// </summary>
        public event DownloadUpdateHandler FileDownloadStarted = (file) => { };
        /// <summary>
        /// Occurs when a file downloading progress
        /// </summary>
        public event DownloadUpdateHandler FileDownloadProgress = (file) => { };
        /// <summary>
        /// Occurs when a file download is completed
        /// </summary>
        public event DownloadUpdateHandler FileDownloaded = (file) => { };
        /// <summary>
        /// Occurs when a file download failed
        /// </summary>
        public event DownloadFailedHandler FileDownloadFailed = (file, exception) => { };

        /// <summary>
        /// Return true if the current instance is disposed
        /// </summary>
        public bool IsDisposed { get; private set; }
        /// <summary>
        /// Return true if the current instance is busy
        /// </summary>
        public bool IsBusy
        {
            get
            {
                return _client.IsBusy;
            }
        }
        /// <summary>
        /// Get or set if the current procedure need to be cancelled when the attempt limit has been reached
        /// </summary>
        public bool StopOnMaxAttempts { get; set; } = true;
        /// <summary>
        /// Get or set the max attempts
        /// </summary>
        public int MaxAttempts { get; set; } = 5;
        /// <summary>
        /// Get the number of files queued
        /// </summary>
        public int QueuedCount
        {
            get
            {
                return _files.Count;
            }
        }

        private WebClient _client;
        private List<WebProgressFile> _files;
        private int _attempts;
        private int _baseTotal;

        /// <summary>
        /// Instantiate a new instance of the class <see cref="WebDownloader"/>
        /// </summary>
        /// <param name="baseUri">Base uri to use</param>
        public WebDownloader(string baseUri)
        {
            if (string.IsNullOrEmpty(baseUri))
                throw new NullReferenceException();

            if (!baseUri.EndsWith("/"))
                baseUri += "/";

            _files = new List<WebProgressFile>();
            _client = new WebClient();
            _client.Proxy = null;

            _client.DownloadProgressChanged += Client_DownloadProgressChanged;
            _client.DownloadFileCompleted += Client_DownloadFileCompleted;

            _client.BaseAddress = baseUri;
        }

        /// <summary>
        /// Enqueue specified uris associated to the specified path
        /// </summary>
        /// <param name="uris">Uris to download</param>
        /// <param name="folderDestinationPath">Folder path where to save the download file</param>
        public void Enqueue(string folderDestinationPath, params string[] uris)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(WebDownloader));

            if (IsBusy)
                throw new WebModuleBusyException();

            foreach (var uri in uris)
            {
                if (string.IsNullOrEmpty(uri) || string.IsNullOrEmpty(folderDestinationPath))
                    throw new EnqueueInvalidInformationsException(uri);

                _files.Add(new WebProgressFile(_client.BaseAddress, uri, folderDestinationPath, false));
            }
        }
        //TODO: same as FtpDownloader for Enqueue function

        /// <summary>
        /// Start the download on the queue
        /// </summary>
        public void DownloadQueue()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(WebDownloader));

            if (IsBusy)
                throw new WebModuleBusyException();

            if (_files.Count == 0)
                return;

            _baseTotal = QueuedCount;

            DownloadNext();
        }
        /// <summary>
        /// Start the download of the queue asynchronously
        /// </summary>
        public void DownloadQueueAsync()
        {
            Task.Factory.StartNew(() => DownloadQueue());
        }

        /// <summary>
        /// Dispose the current instance
        /// </summary>
        public void Dispose()
        {
            if (IsDisposed)
                return;

            QueueDownloaded = null;
            FileDownloadStarted = null;
            FileDownloadProgress = null;
            FileDownloadFailed = null;
            FileDownloaded = null;

            _client.Dispose();
            _files.Clear();
            _client = null;
            _files = null;

            IsDisposed = true;
        }

        private void DownloadNext()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(WebDownloader));

            var file = _files[0];

            FileDownloadStarted(file);

            if (!Directory.Exists(file.DestinationPath))
                Directory.CreateDirectory(file.DestinationPath);

            _client.DownloadFileAsync(new Uri(_client.BaseAddress + file.Path), file.DestinationPath + file.Name);
        }

        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            var file = _files[0];
            file.Progress((ulong)e.TotalBytesToReceive, (ulong)e.BytesReceived);

            FileDownloadProgress(file);
        }
        private void Client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            var file = _files[0];

            if (e.Cancelled || e.Error != null)
            {
                if (++_attempts == MaxAttempts)
                {
                    FileDownloadFailed(file, e.Error ?? new Exception("Download cancelled file(" + file.Name + ") - Attempts exceeded"));
                    if (StopOnMaxAttempts)
                        return;
                }
                else
                {
                    DownloadNext();
                    return;
                }
            }

            FileDownloaded(file);

            _attempts = 0;
            _files.RemoveAt(0);

            QueueDownloadProgress(_baseTotal - QueuedCount, _baseTotal);

            if (_files.Count == 0)
                QueueDownloaded();
            else
                DownloadNext();
        }
    }
}

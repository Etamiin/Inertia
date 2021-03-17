using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Renci.SshNet;

namespace Inertia.Web
{
    /// <summary>
    ///
    /// </summary>
    public class FtpDownloader : IDisposable
    {
        /// <summary>
        /// Occurs when the current queue is completely downloaded
        /// </summary>
        public event BasicAction QueueDownloaded = () => { };
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
                return _client.IsConnected;
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

        private SftpClient _client;
        private FtpCredential _credentials;
        private List<WebProgressFile> _files;
        private int _attempts;

        /// <summary>
        /// Instantiate a new instance of the class <see cref="WebDownloader"/>
        /// </summary>
        /// <param name="credentials">Credentials to use</param>
        /// <param name="bufferSize">Buffer size of the downloader</param>
        public FtpDownloader(FtpCredential credentials, uint bufferSize = 8192)
        {
            _credentials = credentials;
            _client = new SftpClient(credentials.Host, credentials.Port, credentials.Username, credentials.Password);
            _files = new List<WebProgressFile>();

            _client.BufferSize = bufferSize;
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

                _files.Add(new WebProgressFile(_credentials.Host, uri, folderDestinationPath, false));
            }
        }
        //TODO: Enqueue function taking ftp_uri_folder_location and string[] names
        //Example: Enqueue("https://localhost.com/testfolder", "file.txt", "uData.dat", "config.xml");

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

            _client.Connect();
            DownloadNext();

            _client.Disconnect();
            QueueDownloaded();
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
            _credentials = null;

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
            var filePath = file.DestinationPath + file.Name;
            var ftpFile = _client.Get(file.Path);

            FileDownloadStarted(file);

            if (!Directory.Exists(file.DestinationPath))
                Directory.CreateDirectory(file.DestinationPath);
            if (File.Exists(filePath))
                File.Delete(filePath);

            try
            {
                using (var stream = File.Open(filePath, FileMode.OpenOrCreate))
                {
                    _client.DownloadFile(file.Path, stream, (current) =>
                    {
                        file.Progress((ulong)ftpFile.Length, current);
                        FileDownloadProgress(file);
                    });

                    FileDownloaded(file);

                    _attempts = 0;
                    _files.RemoveAt(0);
                }

            }
            catch (Exception ex)
            {
                if (++_attempts == MaxAttempts)
                {
                    FileDownloadFailed(file, ex);
                    if (StopOnMaxAttempts)
                        return;
                }
                else
                {
                    DownloadNext();
                    return;
                }

            }

            if (_files.Count > 0)
                DownloadNext();
        }
    }
}

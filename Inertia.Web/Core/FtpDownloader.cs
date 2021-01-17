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
    /// Represent the ftp downloader class
    /// </summary>
    public class FtpDownloader : IDisposable
    {
        #region Events

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

        #endregion

        #region Public variables

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
                return m_client.IsConnected;
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
                return m_files.Count;
            }
        }

        #endregion

        #region Private variables

        private SftpClient m_client;
        private FtpCredential m_credentials;
        private List<WebProgressFile> m_files;
        private int m_attempts;

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiate a new instance of the class <see cref="WebDownloader"/>
        /// </summary>
        /// <param name="credentials">Credentials to use</param>
        /// <param name="bufferSize">Buffer size of the downloader</param>
        public FtpDownloader(FtpCredential credentials, uint bufferSize = 8192)
        {
            m_credentials = credentials;
            m_client = new SftpClient(credentials.Host, credentials.Port, credentials.Username, credentials.Password);
            m_files = new List<WebProgressFile>();

            m_client.BufferSize = bufferSize;
        }

        #endregion

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

                m_files.Add(new WebProgressFile(m_credentials.Host, uri, folderDestinationPath, false));
            }
        }

        /// <summary>
        /// Start the download on the queue
        /// </summary>
        public void DownloadQueue()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(WebDownloader));

            if (IsBusy)
                throw new WebModuleBusyException();

            if (m_files.Count == 0)
                return;

            m_client.Connect();
            DownloadNext();

            m_client.Disconnect();
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
            m_credentials = null;

            m_client.Dispose();
            m_files.Clear();
            m_client = null;
            m_files = null;

            IsDisposed = true;
        }

        private void DownloadNext()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(WebDownloader));

            var file = m_files[0];
            var filePath = file.DestinationPath + file.Name;
            var ftpFile = m_client.Get(file.Path);

            FileDownloadStarted(file);

            if (!Directory.Exists(file.DestinationPath))
                Directory.CreateDirectory(file.DestinationPath);
            if (File.Exists(filePath))
                File.Delete(filePath);

            try
            {
                using (var stream = File.Open(filePath, FileMode.OpenOrCreate))
                {
                    m_client.DownloadFile(file.Path, stream, (current) =>
                    {
                        file.Progress((ulong)ftpFile.Length, current);
                        FileDownloadProgress(file);
                    });

                    FileDownloaded(file);

                    m_attempts = 0;
                    m_files.RemoveAt(0);
                }

            }
            catch (Exception ex)
            {
                if (++m_attempts == MaxAttempts)
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

            if (m_files.Count > 0)
                DownloadNext();
        }
    }
}

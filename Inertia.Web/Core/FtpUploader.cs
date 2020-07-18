using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Renci.SshNet;

namespace Inertia.Web
{
    /// <summary>
    /// Allows uploading data to the web using FTP credentials
    /// </summary>
    public class FtpUploader : IDisposable
    {
        #region Events

        /// <summary>
        /// Occurs when the current queue is completely uploaded
        /// </summary>
        public event BasicAction QueueUploaded = () => { };
        /// <summary>
        /// Occurs when a file start to be uploaded
        /// </summary>
        public event UploadFileUpdateHandler FileUploadStarted = (file) => { };
        /// <summary>
        /// Occurs when a file uploading progress
        /// </summary>
        public event UploadFileUpdateHandler FileUploadProgress = (file) => { };
        /// <summary>
        /// Occurs when a file upload is completed
        /// </summary>
        public event UploadFileUpdateHandler FileUploaded = (file) => { };
        /// <summary>
        /// Occurs when a file upload failed
        /// </summary>
        public event UploadFileFailedHandler FileUploadFailed = (file, exception) => { };

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
        /// Get the credentials used in the current instance
        /// </summary>
        public FtpCredential Credentials { get; private set; }
        /// <summary>
        /// Get the number of files in the queue
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
        private List<WebProgressFile> m_files;
        private int m_attempts;

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiate a new instance of the class <see cref="FtpUploader"/>
        /// </summary>
        /// <param name="credentials">Credentials to use for uploading data</param>
        public FtpUploader(FtpCredential credentials)
        {
            Credentials = credentials;
            m_client = new SftpClient(Credentials.Host, credentials.Port, credentials.Username, credentials.Password);
            m_files = new List<WebProgressFile>();
        }

        #endregion

        /// <summary>
        /// Enqueue the current path file
        /// </summary>
        /// <param name="path">Path of the file to upload</param>
        public void EnqueueFile(string path)
        {
            EnqueueFile(path, string.Empty);
        }
        /// <summary>
        /// Enqueue the current path file with an destination folder specification
        /// </summary>
        /// <param name="path">Path of the file to upload</param>
        /// <param name="ftpDestinationFolder">Ftp folder destination uri</param>
        public void EnqueueFile(string path, string ftpDestinationFolder)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(FtpUploader));

            if (IsBusy)
                throw new WebModuleBusyException();

            if (!File.Exists(path))
                throw new FileNotFoundException();

            m_files.Add(new WebProgressFile(Credentials.Host, path, ftpDestinationFolder, true));
        }
        /// <summary>
        /// Enqueue all the files in the specified folder path
        /// </summary>
        /// <param name="folderPath">Folder where to get the files</param>
        /// <param name="inheritance">True if sub folders need to be added</param>
        public void EnqueueFolder(string folderPath, bool inheritance)
        {
            EnqueueFolder(folderPath, string.Empty, inheritance);
        }
        /// <summary>
        /// Enqueue all the files in the specified folder path with an destination folder specification
        /// </summary>
        /// <param name="folderPath">Folder where to get the files</param>
        /// <param name="ftpDestinationFolder">Ftp folder destination uri</param>
        /// <param name="inheritance">True if sub folders need to be added</param>
        public void EnqueueFolder(string folderPath, string ftpDestinationFolder, bool inheritance)
        {
            var files = InertiaIO.GetFilesPathFromDirectory(folderPath, inheritance);
            foreach (var filePath in files)
                EnqueueFile(filePath, ftpDestinationFolder + filePath.Replace(folderPath, string.Empty));
        }

        /// <summary>
        /// Start the upload of the queue
        /// </summary>
        public void UploadQueue()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(FtpUploader));

            if (IsBusy)
                throw new WebModuleBusyException();

            m_client.Connect();
            UploadNext();

            m_client.Disconnect();
            QueueUploaded();
        }
        /// <summary>
        /// Start the upload of the queue asynchronously
        /// </summary>
        public void UploadQueueAsync()
        {
            Task.Factory.StartNew(() => UploadQueue());
        }

        /// <summary>
        /// Dispose the current instance
        /// </summary>
        public void Dispose()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(FtpUploader));

            m_client.Dispose();
            m_files.Clear();
            m_client = null;
            m_files = null;
            Credentials = null;

            IsDisposed = true;
        }

        private void UploadNext()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(FtpUploader));

            FileUploadStarted(m_files[0]);

            var folderPath = m_files[0].DestinationPath.Remove(m_files[0].DestinationPath.Length - 1, 1);

            if (!string.IsNullOrEmpty(folderPath))
            {
                var paths = folderPath.Split('/');
                var path = string.Empty;

                for (var i = 0; i < paths.Length; i++)
                {
                    path += paths[i];

                    if (!m_client.Exists(path))
                    {
                        Console.WriteLine(path);
                        m_client.CreateDirectory(path);
                    }

                    path += "/";
                }
            }

            try
            {
                using (var stream = File.Open(m_files[0].Path, FileMode.Open))
                {
                    m_client.UploadFile(stream, m_files[0].DestinationPath + m_files[0].Name, true, (current) => {
                        m_files[0].Progress((ulong)stream.Length, current);
                        FileUploadProgress(m_files[0]);
                    });
                }

                FileUploaded(m_files[0]);

                m_attempts = 0;
                m_files.RemoveAt(0);
            }
            catch (Exception ex) {
                if (++m_attempts == MaxAttempts)
                {
                    FileUploadFailed(m_files[0], ex);
                    if (StopOnMaxAttempts)
                        return;
                }
                else
                {
                    UploadNext();
                    return;
                }
            }

            if (m_files.Count > 0)
                UploadNext();
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Inertia.Internal;

namespace Inertia.Web
{
    public class FtpUploader : IDisposable
    {
        #region Events

        public event InertiaAction QueueUploaded = () => { };
        public event UploadFileStartedHandler FileUploadStarted = (file) => { };
        public event UploadFileProgressHandler FileUploadProgress = (file) => { };
        public event UploadFileCompletedHandler FileUploadCompleted = (file) => { };
        public event UploadFileFailedHandler FileUploadFailed = (file, exception) => { };

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
        public readonly FtpCredential Credentials;

        #endregion

        #region Private variables

        private readonly WebClient Client;
        private readonly List<WebUploadFileData> Files;
        private int CurrentAttempt;

        #endregion

        #region Constructors

        public FtpUploader(FtpCredential credentials)
        {
            Credentials = credentials;
            WebConfig = new WebModuleConfiguration();
            Client = new WebClient();
            Files = new List<WebUploadFileData>();

            Client.Credentials = (NetworkCredential)credentials;
            Client.UploadFileCompleted += Client_UploadFileCompleted;
            Client.UploadProgressChanged += Client_UploadProgressChanged;

            Client.BaseAddress = credentials.CompleteHost;
        }

        #endregion

        public void EnqueueFile(string path)
        {
            EnqueueFile(path, string.Empty);
        }
        public void EnqueueFile(string path, string ftpDestinationFolder)
        {
            if (IsWorking)
                throw new WebModuleBusyException();

            if (!File.Exists(path))
                throw new FileNotFoundException();

            Files.Add(new WebUploadFileData(path, Credentials.Host, ftpDestinationFolder));
        }
        public void EnqueueFolder(string folderPath, bool inheritance)
        {
            EnqueueFolder(folderPath, string.Empty, inheritance);
        }
        public void EnqueueFolder(string folderPath, string ftpDestinationFolder, bool inheritance)
        {
            var files = InertiaIO.GetFilesPathFromDirectory(folderPath, inheritance);
            foreach (var filePath in files)
                EnqueueFile(filePath, ftpDestinationFolder + filePath.Replace(folderPath, string.Empty));
        }

        public void UploadQueue()
        {
            if (IsWorking)
                throw new WebModuleBusyException();

            UploadNext();
        }
        public void UploadQueueAsync()
        {
            Task.Factory.StartNew(() => UploadQueue());
        }

        private void UploadNext()
        {
            FileUploadStarted(Files[0]);

            WebManager.CreateDirectoryOnFtp(Files[0].DestinationFolderUri, Credentials);
            Client.UploadFileAsync(new Uri(Client.BaseAddress + Files[0].DestinationFolderUri + Files[0].Name), WebRequestMethods.Ftp.UploadFile, Files[0].Path);
        }

        private void Client_UploadProgressChanged(object sender, UploadProgressChangedEventArgs e)
        {
            var file = Files[0];
            file.Progress(e);

            FileUploadProgress(file);
        }
        private void Client_UploadFileCompleted(object sender, System.Net.UploadFileCompletedEventArgs e)
        {
            var file = Files[0];

            if (e.Cancelled || e.Error != null)
            {
                if (++CurrentAttempt == WebConfig.MaxRetryAttempt)
                {
                    FileUploadFailed(file, e.Error ?? new Exception("Download cancelled file(" + file.Name + ")"));
                    if (WebConfig.StopOnFail)
                        return;
                }
                else {
                    UploadNext();
                    return;
                }
            }

            FileUploadCompleted(file);

            CurrentAttempt = 0;
            Files.RemoveAt(0);

            if (Files.Count == 0)
                QueueUploaded();
            else
                UploadNext();
        }

        public void Dispose()
        {
            Client.Dispose();
            WebConfig.Dispose();
            Credentials.Dispose();
            WebConfig = null;
        }
    }
}

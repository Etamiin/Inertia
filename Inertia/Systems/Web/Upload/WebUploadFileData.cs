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
    public class WebUploadFileData : IDisposable
    {
        #region Public variables

        public string Path { get; private set; }
        public readonly string Name;
        public string DestinationFolderUri { get; private set; }

        public long TotalBytes { get; internal set; }
        public long CurrentBytes { get; internal set; }

        public int Progression { get; internal set; }

        public float TotalMB
        {
            get
            {
                return TotalBytes / 1000000f;
            }
        }
        public float CurrentMB
        {
            get
            {
                return CurrentBytes / 1000000f;
            }
        }

        #endregion

        #region Constructors

        internal WebUploadFileData(string path, string host, string ftpDestinationFolder)
        {
            Path = path;
            Name = new FileInfo(Path).Name;

            StringConventionNormalizer.NormalizeFolderUri(ref host, ref Name, ref ftpDestinationFolder);
            DestinationFolderUri = ftpDestinationFolder;
        }

        #endregion

        internal void Progress(UploadProgressChangedEventArgs e)
        {
            Progression = e.ProgressPercentage;
            TotalBytes = e.TotalBytesToSend;
            CurrentBytes = e.BytesSent;
        }

        public void Dispose()
        {
            Path = null;
            DestinationFolderUri = null;
        }
    }
}

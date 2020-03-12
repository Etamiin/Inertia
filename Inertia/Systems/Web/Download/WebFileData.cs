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
    public class WebFileData : IDisposable
    {
        #region Public variables

        public string CompleteUri { get; private set; }
        public string Name { get; private set; }
        public string Destination { get; private set; }

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

        internal WebFileData(string host, string uri, string folderDestinationPath)
        {
            StringConventionNormalizer.NormalizeUri(ref host, ref uri);
            folderDestinationPath = StringConventionNormalizer.GetNormalizedFolderPath(folderDestinationPath);

            var charIndex = uri.LastIndexOf('/');

            CompleteUri = uri;
            Name = uri.Substring(charIndex + 1, uri.Length - charIndex - 1);
            Destination = folderDestinationPath;

            if (CompleteUri.Contains("/"))
            {
                Destination += CompleteUri;
                Destination = Destination.Replace("/", @"\").Replace(Name, string.Empty);
            }
        }

        #endregion

        internal void Progress(DownloadProgressChangedEventArgs e)
        {
            Progression = e.ProgressPercentage;
            TotalBytes = e.TotalBytesToReceive;
            CurrentBytes = e.BytesReceived;
        }

        public void Dispose()
        {
            CompleteUri = null;
            Name = null;
            Destination = null;
        }
    }
}
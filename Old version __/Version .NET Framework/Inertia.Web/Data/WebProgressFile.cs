using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Web
{
    //TODO: review fields
    /// <summary>
    /// 
    /// </summary>
    public class WebProgressFile
    {
        /// <summary>
        /// Get the path of the current file
        /// </summary>
        public readonly string Path;
        /// <summary>
        /// Get the name of the current file
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// Get the destination path of the current file
        /// </summary>
        public readonly string DestinationPath;
        /// <summary>
        /// Get the total bytes 
        /// </summary>
        public ulong TotalBytes { get; private set; }
        /// <summary>
        /// Get the current bytes
        /// </summary>
        public ulong CurrentBytes { get; private set; }
        /// <summary>
        /// Get the percentage progression
        /// </summary>
        public int ProgressPercentage { get; private set; }

        internal WebProgressFile(string host, string path, string destinationFolder, bool isUploader)
        {
            if (isUploader)
            {
                Path = path;
                Name = new FileInfo(Path).Name;
                DestinationPath = destinationFolder.NormalizeFolderUriForWebFile(host, Name);
            }
            else {
                path = path.NormalizeUriForWebFile(host);
                destinationFolder = destinationFolder.VerifyPathForFolder();

                var charIndex = path.LastIndexOf('/');

                Path = path;
                Name = path.Substring(charIndex + 1, path.Length - charIndex - 1);
                DestinationPath = destinationFolder;

                if (Path.Contains("/"))
                {
                    DestinationPath += Path;
                    DestinationPath = DestinationPath.Replace("/", @"\").Replace(Name, string.Empty);
                }
            }
        }

        internal void Progress(ulong total, ulong current)
        {
            TotalBytes = total;
            CurrentBytes = current;

            if (CurrentBytes > TotalBytes)
                TotalBytes = CurrentBytes;

            ProgressPercentage = (int)((float)CurrentBytes / TotalBytes * 100);
        }
    }
}

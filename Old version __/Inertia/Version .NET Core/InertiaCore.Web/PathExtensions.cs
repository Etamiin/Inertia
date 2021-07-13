using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia
{
    //TODO: to rework (reset from Inertia)
    internal static class PathExtensions
    {
        internal static string VerifyPathForFolder(this string folderPath)
        {
            var path = folderPath;

            if (!File.Exists(path) || !File.GetAttributes(path).HasFlag(FileAttributes.Directory))
            {
                var info = new DirectoryInfo(path);
                path = info.FullName;
            }

            if (!path.EndsWith(@"\"))
                path += @"\";

            return path;
        }
        internal static string NormalizeUriForWebFile(this string uri, string host)
        {
            var normalizedUri = uri.Replace(host, string.Empty).Replace(@"\", "/");

            if (normalizedUri.StartsWith("/"))
                normalizedUri = normalizedUri.Remove(0, 1);
            if (normalizedUri.EndsWith("/"))
                normalizedUri = normalizedUri.Remove(normalizedUri.Length - 1);

            return normalizedUri;
        }
        internal static string NormalizeFolderUriForWebFile(this string uri, string host, string fileName)
        {
            var normalizedUri = uri.Replace(host, string.Empty).Replace(@"\", "/").Replace(fileName, string.Empty);

            if (normalizedUri.StartsWith("/"))
                normalizedUri = normalizedUri.Remove(0, 1);
            if (!normalizedUri.EndsWith("/"))
                normalizedUri += "/";

            return normalizedUri;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inertia.Internal
{
    internal static class StringConventionNormalizer
    {
        public static void NormalizeHostAdressUri(ref string uri)
        {
            if (!uri.EndsWith("/"))
                uri += "/";
        }
        public static void NormalizeFtpHostAdressUri(ref string uri)
        {
            NormalizeHostAdressUri(ref uri);

            if (!uri.StartsWith("ftp://") || !uri.StartsWith("ftps://"))
                uri = "ftp://" + uri;
        }

        public static void NormalizeUri(ref string host, ref string uri)
        {
            uri = uri.Replace(host, string.Empty).Replace(@"\", "/");

            if (uri.StartsWith("/"))
                uri = uri.Remove(0, 1);
            if (uri.EndsWith("/"))
                uri = uri.Remove(uri.Length - 1);
        }
        public static void NormalizeFolderUri(ref string host, ref string fileName, ref string uri)
        {
            uri = uri.Replace(host, string.Empty).Replace(@"\", "/").Replace(fileName, string.Empty);

            if (uri.StartsWith("/"))
                uri = uri.Remove(0, 1);
            if (!uri.EndsWith("/"))
                uri += "/";
        }

        public static string GetNormalizedFolderPath(string folderPath)
        {
            var normalized = folderPath;

            if (!File.Exists(normalized) || !File.GetAttributes(normalized).HasFlag(FileAttributes.Directory))
            {
                var info = new DirectoryInfo(normalized);
                normalized = info.FullName;
            }

            if (!normalized.EndsWith(@"\"))
                normalized += @"\";

            return normalized;
        }
    }
}
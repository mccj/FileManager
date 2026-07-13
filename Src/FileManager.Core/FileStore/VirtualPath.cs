using System;
using System.Linq;

namespace FileManager.FileStorage
{
    public static class VirtualPath
    {
        public static string Normalize(string path)
        {
            if (path == null)
                return null;

            var normalized = path.Replace('\\', '/').Trim('/');
            ValidateNormalized(normalized);
            return normalized;
        }

        public static void ValidateNormalized(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            if (path.StartsWith("/", StringComparison.Ordinal))
                throw new FileStoreException("Absolute paths are not supported.");

            if (path.IndexOf('\0') >= 0)
                throw new FileStoreException("Path contains invalid characters.");

            foreach (var segment in path.Split('/'))
            {
                if (segment == "..")
                    throw new FileStoreException("Parent traversal (..) is not supported.");
            }
        }

        public static string ValidateFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new FileStoreException("File name is required.");

            fileName = fileName.Replace('\\', '/').Trim();
            if (fileName.Contains("/"))
                throw new FileStoreException("File name must not contain path separators.");

            if (fileName == ".." || fileName == ".")
                throw new FileStoreException("Invalid file name.");

            if (fileName.IndexOf('\0') >= 0)
                throw new FileStoreException("File name contains invalid characters.");

            return fileName;
        }

        public static bool IsExtensionAllowed(string fileName, string[] allowedExtensions, bool ignoreCase = true)
        {
            if (allowedExtensions == null || allowedExtensions.Length == 0)
                return true;

            var extension = System.IO.Path.GetExtension(fileName)?.TrimStart('.');
            if (string.IsNullOrEmpty(extension))
                return false;

            var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            return allowedExtensions.Any(item =>
                string.Equals(item?.TrimStart('.'), extension, comparison));
        }
    }
}

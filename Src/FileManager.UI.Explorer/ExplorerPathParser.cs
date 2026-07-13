using FileManager.FileStorage;
using System.Text.RegularExpressions;

namespace FileManager
{
    internal static class ExplorerPathParser
    {
        private static readonly Regex DocumentViewerRegex = new Regex("(?<stateId>\\S*?)/(?<drive>.+?)/(?<path>.*)", RegexOptions.Compiled);

        public static bool TryParsePath(string drivePath, IFileStore fileStore, out string path)
        {
            return DrivePath.TryParseExplorerPath(drivePath, fileStore, out path);
        }

        public static string ParsePath(string drivePath, IFileStore fileStore)
        {
            if (!TryParsePath(drivePath, fileStore, out var path))
                throw new FileStoreException($"Invalid explorer path '{drivePath}'.");
            return path;
        }

        public static bool TryParseDocumentViewerPath(string fileName, out string path)
        {
            path = null;
            var match = DocumentViewerRegex.Match(fileName ?? string.Empty);
            if (!match.Success)
                return false;

            path = match.Groups["path"].Value;
            return true;
        }

        public static string CombinePath(string basePath, string itemName)
        {
            var normalizedItem = (itemName ?? string.Empty).Trim().TrimEnd('\\', '/');
            if (string.IsNullOrEmpty(basePath))
                return string.IsNullOrEmpty(normalizedItem) ? string.Empty : VirtualPath.Normalize(normalizedItem);
            if (string.IsNullOrEmpty(normalizedItem))
                return VirtualPath.Normalize(basePath);
            return VirtualPath.Normalize(basePath.TrimEnd('/') + "/" + normalizedItem.TrimStart('/'));
        }
    }
}

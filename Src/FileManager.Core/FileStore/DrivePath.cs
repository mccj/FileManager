using System;

namespace FileManager.FileStorage
{
    /// <summary>
    /// Helpers for multi-drive paths and Explorer <c>[drive]:path</c> syntax.
    /// </summary>
    public static class DrivePath
    {
        public static bool TryParseExplorerPath(string drivePath, out string storePath)
        {
            storePath = string.Empty;
            if (string.IsNullOrWhiteSpace(drivePath))
                return true;

            var open = drivePath.IndexOf('[');
            var close = drivePath.IndexOf(']');
            if (open != 0 || close <= 1)
                return false;

            var drive = drivePath.Substring(1, close - 1).Trim();
            if (drivePath.Length <= close + 1 || drivePath[close + 1] != ':')
                return false;

            var path = drivePath.Substring(close + 2);
            storePath = Combine(drive, path);
            return true;
        }

        public static bool TryParseExplorerPath(string drivePath, IFileStore fileStore, out string storePath)
        {
            if (!TryParseExplorerPath(drivePath, out storePath))
                return false;

            storePath = ToBackingStorePath(storePath, fileStore);
            return true;
        }

        /// <summary>
        /// Maps a parsed explorer path to the path expected by the backing store.
        /// Single-drive stores ignore the UI root-folder name (for example <c>[Root]:</c> becomes <c>""</c>).
        /// </summary>
        public static string ToBackingStorePath(string parsedExplorerPath, IFileStore fileStore)
        {
            parsedExplorerPath = VirtualPath.Normalize(parsedExplorerPath) ?? string.Empty;
            if (fileStore is IMultiDriveFileStore)
                return parsedExplorerPath;

            if (string.IsNullOrEmpty(parsedExplorerPath))
                return string.Empty;

            if (TrySplit(parsedExplorerPath, out _, out var relative))
                return relative ?? string.Empty;

            return string.Empty;
        }

        public static string Combine(string drive, string path)
        {
            drive = drive?.Trim();
            path = path ?? string.Empty;

            if (string.IsNullOrEmpty(drive))
                return VirtualPath.Normalize(path);

            if (string.IsNullOrEmpty(path))
                return VirtualPath.Normalize(drive);

            return VirtualPath.Normalize(drive.TrimEnd('/') + "/" + path.TrimStart('/'));
        }

        public static bool TrySplit(string storePath, out string drive, out string path)
        {
            drive = null;
            path = string.Empty;
            storePath = VirtualPath.Normalize(storePath);
            if (string.IsNullOrEmpty(storePath))
                return false;

            var slash = storePath.IndexOf('/');
            if (slash < 0)
            {
                drive = storePath;
                return true;
            }

            drive = storePath.Substring(0, slash);
            path = storePath.Substring(slash + 1);
            return true;
        }
    }
}

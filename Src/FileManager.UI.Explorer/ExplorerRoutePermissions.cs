using FileManager.FileStorage;
using System;

namespace FileManager
{
    internal static class ExplorerRoutePermissions
    {
        internal static FileStorePermissionGuard Create(IServiceProvider services)
        {
            return new FileStorePermissionGuard(
                (IFileStore)services.GetService(typeof(IFileStore)),
                (DefaultPermissionHandle)services.GetService(typeof(DefaultPermissionHandle)));
        }

        internal static bool TryParsePath(IServiceProvider services, string drivePath, out string path)
        {
            var fileStore = (IFileStore)services.GetService(typeof(IFileStore));
            return ExplorerPathParser.TryParsePath(drivePath, fileStore, out path);
        }

        internal static string ParsePath(IServiceProvider services, string drivePath)
        {
            return ExplorerPathParser.ParsePath(drivePath, (IFileStore)services.GetService(typeof(IFileStore)));
        }

        internal static string ItemPath(string directoryPath, string itemName)
        {
            var name = (itemName ?? string.Empty).TrimEnd('\\', '/');
            return string.IsNullOrEmpty(directoryPath) ? name : directoryPath + "/" + name;
        }
    }
}

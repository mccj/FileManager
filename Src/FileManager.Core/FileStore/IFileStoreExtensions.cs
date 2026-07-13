using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FileManager.FileStorage
{
    public static class IFileStoreExtensions
    {
        public static string Combine(this IFileStore fileStore, params string[] paths)
        {
            if (paths.Length == 0)
                return null;

            var normalizedParts =
                paths
                    .Select(x => fileStore.NormalizePath(x))
                    .Where(x => !String.IsNullOrEmpty(x))
                    .ToArray();

            var combined = String.Join("/", normalizedParts);
            return combined;
        }

        public static string NormalizePath(this IFileStore fileStore, string path)
        {
            return VirtualPath.Normalize(path);
        }

        public static async Task CopyDirectoryAsync(this IFileStore fileStore, string sourcePath, string targetPath, CancellationToken cancellationToken = default)
        {
            sourcePath = fileStore.NormalizePath(sourcePath);
            targetPath = fileStore.NormalizePath(targetPath);

            await fileStore.TryCreateDirectoryAsync(targetPath, cancellationToken);
            var entries = await fileStore.GetDirectoryContentAsync(sourcePath, includeSubDirectories: false, cancellationToken);
            foreach (var entry in entries)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var sourceItemPath = fileStore.Combine(sourcePath, entry.Name);
                var targetItemPath = fileStore.Combine(targetPath, entry.Name);
                if (entry.IsDirectory)
                    await CopyDirectoryAsync(fileStore, sourceItemPath, targetItemPath, cancellationToken);
                else
                    await fileStore.CopyFileAsync(sourceItemPath, targetItemPath, cancellationToken);
            }
        }
    }
}

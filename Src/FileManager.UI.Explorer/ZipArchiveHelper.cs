using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace FileManager
{
    internal static class ZipArchiveHelper
    {
        public static async Task CreateZipAsync(
            FileStorage.IFileStore fileStore,
            string basePath,
            IEnumerable<string> itemNames,
            Stream outputStream)
        {
            using (var archive = new ZipArchive(outputStream, ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach (var itemName in itemNames)
                {
                    var isDirectory = itemName.EndsWith("\\", StringComparison.Ordinal) || itemName.EndsWith("/", StringComparison.Ordinal);
                    var name = itemName.TrimEnd('\\', '/');
                    var itemPath = string.IsNullOrEmpty(basePath) ? name : $"{basePath}/{name}";

                    if (isDirectory)
                        await AddDirectoryToZipAsync(fileStore, itemPath, name + "/", archive);
                    else
                        await AddFileToZipAsync(fileStore, itemPath, name, archive);
                }
            }
        }

        public static async Task AddItemsToZipAsync(
            FileStorage.IFileStore fileStore,
            string basePath,
            IEnumerable<string> itemNames,
            string zipFilePath)
        {
            using (var zipStream = await fileStore.GetFileStreamAsync(zipFilePath))
            using (var outputStream = new MemoryStream())
            {
                await zipStream.CopyToAsync(outputStream);
                outputStream.Position = 0;

                using (var readArchive = new ZipArchive(outputStream, ZipArchiveMode.Read, leaveOpen: true))
                using (var writeStream = new MemoryStream())
                {
                    using (var writeArchive = new ZipArchive(writeStream, ZipArchiveMode.Create, leaveOpen: true))
                    {
                        foreach (var entry in readArchive.Entries)
                        {
                            var newEntry = writeArchive.CreateEntry(entry.FullName, CompressionLevel.Optimal);
                            using (var source = entry.Open())
                            using (var target = newEntry.Open())
                                await source.CopyToAsync(target);
                        }

                        foreach (var itemName in itemNames)
                        {
                            var isDirectory = itemName.EndsWith("\\", StringComparison.Ordinal) || itemName.EndsWith("/", StringComparison.Ordinal);
                            var name = itemName.TrimEnd('\\', '/');
                            var itemPath = string.IsNullOrEmpty(basePath) ? name : $"{basePath}/{name}";

                            if (isDirectory)
                                await AddDirectoryToZipAsync(fileStore, itemPath, name + "/", writeArchive);
                            else
                                await AddFileToZipAsync(fileStore, itemPath, name, writeArchive);
                        }
                    }

                    writeStream.Position = 0;
                    await fileStore.CreateFileFromStreamAsync(zipFilePath, writeStream, overwrite: true);
                }
            }
        }

        public static async Task ExtractAllAsync(
            FileStorage.IFileStore fileStore,
            string zipFilePath,
            string destinationPath)
        {
            using (var zipStream = await fileStore.GetFileStreamAsync(zipFilePath))
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Read, leaveOpen: false))
            {
                foreach (var entry in archive.Entries)
                {
                    if (string.IsNullOrEmpty(entry.Name))
                        continue;

                    var entryPath = string.IsNullOrEmpty(destinationPath)
                        ? entry.FullName.Replace('\\', '/')
                        : $"{destinationPath}/{entry.FullName}".Replace('\\', '/');

                    if (entry.FullName.EndsWith("/", StringComparison.Ordinal) || entry.FullName.EndsWith("\\", StringComparison.Ordinal))
                    {
                        await fileStore.TryCreateDirectoryAsync(entryPath.TrimEnd('/'));
                        continue;
                    }

                    var directoryPath = entryPath.Substring(0, entryPath.LastIndexOf('/'));
                    if (!string.IsNullOrEmpty(directoryPath))
                        await fileStore.TryCreateDirectoryAsync(directoryPath);

                    using (var entryStream = entry.Open())
                    using (var buffer = new MemoryStream())
                    {
                        await entryStream.CopyToAsync(buffer);
                        buffer.Position = 0;
                        await fileStore.CreateFileFromStreamAsync(entryPath, buffer, overwrite: true);
                    }
                }
            }
        }

        private static async Task AddDirectoryToZipAsync(
            FileStorage.IFileStore fileStore,
            string directoryPath,
            string zipPrefix,
            ZipArchive archive)
        {
            archive.CreateEntry(zipPrefix.Replace('\\', '/'), CompressionLevel.Optimal);

            var entries = await fileStore.GetDirectoryContentAsync(directoryPath, includeSubDirectories: false);
            foreach (var entry in entries)
            {
                var childPath = string.IsNullOrEmpty(directoryPath) ? entry.Name : $"{directoryPath}/{entry.Name}";
                var childZipPath = zipPrefix + entry.Name;

                if (entry.IsDirectory)
                    await AddDirectoryToZipAsync(fileStore, childPath, childZipPath + "/", archive);
                else
                    await AddFileToZipAsync(fileStore, childPath, childZipPath, archive);
            }
        }

        private static async Task AddFileToZipAsync(
            FileStorage.IFileStore fileStore,
            string filePath,
            string zipEntryName,
            ZipArchive archive)
        {
            var zipEntry = archive.CreateEntry(zipEntryName.Replace('\\', '/'), CompressionLevel.Optimal);
            using (var source = await fileStore.GetFileStreamAsync(filePath))
            using (var target = zipEntry.Open())
                await source.CopyToAsync(target);
        }
    }
}

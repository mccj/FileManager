using FileManager.FileStorage;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#if NET8_0_OR_GREATER
using AspNetCoreDashboard.Abstractions;
#endif

namespace FileManager
{
    internal sealed class ElFinderConnector
    {
        private const string VolumeId = "fm1";
        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() },
            NullValueHandling = NullValueHandling.Ignore
        };

        private readonly IFileStore _fileStore;
        private readonly FileStorePermissionGuard _guard;

        public ElFinderConnector(IFileStore fileStore, FileStorePermissionGuard guard = null)
        {
            _fileStore = fileStore ?? throw new ArgumentNullException(nameof(fileStore));
            _guard = guard;
        }

        public async Task<object> ExecuteAsync(
            string cmd,
            string target,
            string name,
            string oldName,
            string newName,
            string source,
            string targetPath,
            string content,
            IEnumerable<Stream> uploadStreams,
            IEnumerable<string> uploadNames,
            bool cut,
            bool init,
            bool tree,
            CancellationToken cancellationToken)
        {
            try
            {
                switch ((cmd ?? string.Empty).ToLowerInvariant())
                {
                    case "open":
                        return await OpenAsync(target, init, tree, cancellationToken);
                    case "tree":
                        return await TreeAsync(target, cancellationToken);
                    case "parents":
                        return await ParentsAsync(target, cancellationToken);
                    case "mkdir":
                        return await MkdirAsync(target, name, cancellationToken);
                    case "mkfile":
                        return await MkFileAsync(target, name, cancellationToken);
                    case "rm":
                        return await RemoveAsync(ParseHashes(target), cancellationToken);
                    case "rename":
                        return await RenameAsync(target, name, cancellationToken);
                    case "upload":
                        return await UploadAsync(target, uploadStreams, uploadNames, cancellationToken);
                    case "file":
                        return await DownloadAsync(target, cancellationToken);
                    case "get":
                        return await GetContentAsync(target, cancellationToken);
                    case "put":
                        return await PutContentAsync(target, content, cancellationToken);
                    case "paste":
                        return await PasteAsync(source, targetPath, cut, cancellationToken);
                    case "duplicate":
                        return await DuplicateAsync(target, cancellationToken);
                    case "size":
                        return await SizeAsync(ParseHashes(target), cancellationToken);
                    default:
                        return Error($"Unsupported command '{cmd}'.");
                }
            }
            catch (FileStoreException ex)
            {
                return Error(ex.Message);
            }
        }

        public string Serialize(object payload) => JsonConvert.SerializeObject(payload, JsonSettings);

        private async Task<object> OpenAsync(string target, bool init, bool tree, CancellationToken cancellationToken)
        {
            var path = DecodeHash(target);
            if (_guard != null)
                await _guard.EnsureListAsync(path, cancellationToken);
            var cwd = await BuildDirectoryAsync(path, cancellationToken);
            var entries = await _fileStore.GetDirectoryContentAsync(path, includeSubDirectories: false, cancellationToken);
            var files = new List<object>();
            foreach (var entry in entries.OrderBy(e => !e.IsDirectory).ThenBy(e => e.Name, StringComparer.OrdinalIgnoreCase))
                files.Add(await BuildEntryAsync(entry, path, cancellationToken));

            if (tree)
            {
                foreach (var entry in entries.Where(e => e.IsDirectory).OrderBy(e => e.Name, StringComparer.OrdinalIgnoreCase))
                    await AppendDirectoryTreeAsync(files, entry.Path, cancellationToken);
            }

            var options = new Dictionary<string, object>
            {
                ["path"] = string.IsNullOrEmpty(path) ? "Home" : path.Replace('/', '\\'),
                ["url"] = string.Empty,
                ["disabled"] = Array.Empty<string>(),
                ["separator"] = "/",
                ["copyOverwrite"] = 1,
                ["uploadOverwrite"] = 1
            };

            if (init)
            {
                var initResponse = new Dictionary<string, object>
                {
                    ["cwd"] = cwd,
                    ["files"] = files,
                    ["options"] = options,
                    ["api"] = 2.1,
                    ["uplMaxSize"] = "2G",
                    ["netDrivers"] = Array.Empty<object>(),
                    ["maxTargets"] = 1000
                };

                if (tree)
                {
                    var treeEntries = new List<object> { cwd };
                    foreach (var entry in files.OfType<Dictionary<string, object>>())
                    {
                        if (entry.TryGetValue("mime", out var mime) && string.Equals(mime as string, "directory", StringComparison.Ordinal))
                            treeEntries.Add(entry);
                    }

                    initResponse["tree"] = treeEntries;
                }

                return initResponse;
            }

            return new { cwd, files, options, api = 2.1 };
        }

        private async Task<object> TreeAsync(string target, CancellationToken cancellationToken)
        {
            var path = DecodeHash(target);
            if (_guard != null)
                await _guard.EnsureListAsync(path, cancellationToken);
            var tree = new List<object>();
            var children = await _fileStore.GetDirectoryContentAsync(path, includeSubDirectories: false, cancellationToken);
            foreach (var child in children.Where(e => e.IsDirectory).OrderBy(e => e.Name, StringComparer.OrdinalIgnoreCase))
                tree.Add(await BuildEntryAsync(child, path, cancellationToken));

            return new { tree };
        }

        private async Task<object> ParentsAsync(string target, CancellationToken cancellationToken)
        {
            var path = DecodeHash(target);
            if (_guard != null)
                await _guard.EnsureListAsync(path, cancellationToken);
            var tree = new List<object>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            async Task AddParentLevelAsync(string parentPath)
            {
                if (seen.Add(parentPath))
                    tree.Add(await BuildDirectoryAsync(parentPath, cancellationToken));

                var children = await _fileStore.GetDirectoryContentAsync(parentPath, includeSubDirectories: false, cancellationToken);
                foreach (var child in children.Where(e => e.IsDirectory).OrderBy(e => e.Name, StringComparer.OrdinalIgnoreCase))
                {
                    var childPath = child.Path ?? string.Empty;
                    if (seen.Add(childPath))
                        tree.Add(await BuildEntryAsync(child, parentPath, cancellationToken));
                }
            }

            if (string.IsNullOrEmpty(path))
            {
                await AddParentLevelAsync(string.Empty);
            }
            else
            {
                var segments = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                var current = string.Empty;
                for (var i = 0; i < segments.Length; i++)
                {
                    await AddParentLevelAsync(current);
                    current = i == 0 ? segments[0] : current + "/" + segments[i];
                }

                if (seen.Add(path))
                    tree.Add(await BuildDirectoryAsync(path, cancellationToken));
            }

            return new { tree };
        }

        private async Task<object> MkdirAsync(string target, string name, CancellationToken cancellationToken)
        {
            name = VirtualPath.ValidateFileName(name);
            var parent = DecodeHash(target);
            var path = _fileStore.Combine(parent, name);
            if (_guard != null)
                await _guard.EnsureMkdirAsync(path, cancellationToken);
            await _fileStore.TryCreateDirectoryAsync(path, cancellationToken);
            return new { added = new[] { await BuildDirectoryAsync(path, cancellationToken) } };
        }

        private async Task<object> MkFileAsync(string target, string name, CancellationToken cancellationToken)
        {
            name = VirtualPath.ValidateFileName(name);
            var parent = DecodeHash(target);
            var path = _fileStore.Combine(parent, name);
            if (_guard != null)
                await _guard.EnsureUploadAsync(parent, cancellationToken);
            await _fileStore.CreateFileFromStreamAsync(path, new MemoryStream(Array.Empty<byte>()), cancellationToken: cancellationToken);
            var entry = await _fileStore.GetFileInfoAsync(path, cancellationToken);
            return new { added = new[] { await BuildEntryAsync(entry, parent, cancellationToken) } };
        }

        private async Task<object> RemoveAsync(IEnumerable<string> hashes, CancellationToken cancellationToken)
        {
            var removed = new List<string>();
            foreach (var hash in hashes)
            {
                var path = DecodeHash(hash);
                if (_guard != null)
                    await _guard.EnsureDeleteAsync(path, cancellationToken);
                if (await _fileStore.GetDirectoryInfoAsync(path, cancellationToken) != null)
                    await _fileStore.TryDeleteDirectoryAsync(path, cancellationToken);
                else
                    await _fileStore.TryDeleteFileAsync(path, cancellationToken);
                removed.Add(hash);
            }

            return new { removed = removed.ToArray() };
        }

        private async Task<object> RenameAsync(string target, string name, CancellationToken cancellationToken)
        {
            name = VirtualPath.ValidateFileName(name);
            var path = DecodeHash(target);
            if (_guard != null)
                await _guard.EnsureRenameAsync(path, cancellationToken);
            var parent = GetParentPath(path);
            var newPath = _fileStore.Combine(parent, name);
            if (await _fileStore.GetDirectoryInfoAsync(path, cancellationToken) != null)
            {
                if (await _fileStore.GetDirectoryInfoAsync(newPath, cancellationToken) != null || await _fileStore.GetFileInfoAsync(newPath, cancellationToken) != null)
                    throw new FileStoreException($"Cannot rename because '{name}' already exists.");
                await _fileStore.MoveDirectoryAsync(path, newPath, cancellationToken);
                return new { added = new[] { await BuildDirectoryAsync(newPath, cancellationToken) }, removed = new[] { target } };
            }

            await _fileStore.MoveFileAsync(path, newPath, cancellationToken);
            var entry = await _fileStore.GetFileInfoAsync(newPath, cancellationToken);
            return new { added = new[] { await BuildEntryAsync(entry, parent, cancellationToken) }, removed = new[] { target } };
        }

        private async Task<object> UploadAsync(string target, IEnumerable<Stream> streams, IEnumerable<string> names, CancellationToken cancellationToken)
        {
            var parent = DecodeHash(target);
            if (_guard != null)
                await _guard.EnsureUploadAsync(parent, cancellationToken);
            var added = new List<object>();
            var streamList = streams?.ToArray() ?? Array.Empty<Stream>();
            var nameList = names?.ToArray() ?? Array.Empty<string>();
            for (var i = 0; i < streamList.Length; i++)
            {
                var fileName = i < nameList.Length ? nameList[i] : "upload.bin";
                fileName = VirtualPath.ValidateFileName(Path.GetFileName(fileName));
                var path = _fileStore.Combine(parent, fileName);
                await _fileStore.CreateFileFromStreamAsync(path, streamList[i], overwrite: true, cancellationToken);
                var entry = await _fileStore.GetFileInfoAsync(path, cancellationToken);
                added.Add(await BuildEntryAsync(entry, parent, cancellationToken));
            }

            return new { added };
        }

        private async Task<object> DownloadAsync(string target, CancellationToken cancellationToken)
        {
            var path = DecodeHash(target);
            if (_guard != null)
                await _guard.EnsureDownloadAsync(path, cancellationToken);
            var stream = await _fileStore.GetFileStreamAsync(path, cancellationToken);
            return new ElFinderFileDownload(path, stream);
        }

        private async Task<object> GetContentAsync(string target, CancellationToken cancellationToken)
        {
            var path = DecodeHash(target);
            await using var stream = await _fileStore.GetFileStreamAsync(path, cancellationToken);
            using var reader = new StreamReader(stream);
            var content = await reader.ReadToEndAsync();
            return new { content };
        }

        private async Task<object> PutContentAsync(string target, string content, CancellationToken cancellationToken)
        {
            var path = DecodeHash(target);
            if (_guard != null)
                await _guard.EnsureEditAsync(path, cancellationToken);
            var bytes = Encoding.UTF8.GetBytes(content ?? string.Empty);
            await _fileStore.CreateFileFromStreamAsync(path, new MemoryStream(bytes), overwrite: true, cancellationToken);
            var entry = await _fileStore.GetFileInfoAsync(path, cancellationToken);
            return new { changed = new[] { await BuildEntryAsync(entry, GetParentPath(path), cancellationToken) } };
        }

        private async Task<object> PasteAsync(string sourceHashes, string destinationHash, bool cmdCut, CancellationToken cancellationToken)
        {
            var destination = DecodeHash(destinationHash);
            var added = new List<object>();
            var removed = new List<string>();
            foreach (var hash in ParseHashes(sourceHashes))
            {
                var sourcePath = DecodeHash(hash);
                var name = Path.GetFileName(sourcePath);
                var targetPath = _fileStore.Combine(destination, name);
                if (_guard != null)
                {
                    if (cmdCut)
                        await _guard.EnsureMoveAsync(sourcePath, destination, cancellationToken);
                    else
                        await _guard.EnsureCopyAsync(sourcePath, destination, cancellationToken);
                }
                if (await _fileStore.GetDirectoryInfoAsync(sourcePath, cancellationToken) != null)
                {
                    if (cmdCut)
                        await _fileStore.MoveDirectoryAsync(sourcePath, targetPath, cancellationToken);
                    else
                        await _fileStore.CopyDirectoryAsync(sourcePath, targetPath, cancellationToken);
                    added.Add(await BuildDirectoryAsync(targetPath, cancellationToken));
                }
                else
                {
                    if (cmdCut)
                        await _fileStore.MoveFileAsync(sourcePath, targetPath, cancellationToken);
                    else
                        await _fileStore.CopyFileAsync(sourcePath, targetPath, cancellationToken);
                    var entry = await _fileStore.GetFileInfoAsync(targetPath, cancellationToken);
                    added.Add(await BuildEntryAsync(entry, destination, cancellationToken));
                }

                if (cmdCut)
                    removed.Add(hash);
            }

            return new { added, removed };
        }

        private async Task<object> DuplicateAsync(string target, CancellationToken cancellationToken)
        {
            var path = DecodeHash(target);
            var parent = GetParentPath(path);
            if (_guard != null)
                await _guard.EnsureCopyAsync(path, parent, cancellationToken);
            var name = Path.GetFileName(path);
            var extension = Path.GetExtension(name);
            var baseName = string.IsNullOrEmpty(extension) ? name : name.Substring(0, name.Length - extension.Length);
            var copyName = baseName + " copy" + extension;
            var copyPath = _fileStore.Combine(parent, copyName);
            await _fileStore.CopyFileAsync(path, copyPath, cancellationToken);
            var entry = await _fileStore.GetFileInfoAsync(copyPath, cancellationToken);
            return new { added = new[] { await BuildEntryAsync(entry, parent, cancellationToken) } };
        }

        private async Task<object> SizeAsync(IEnumerable<string> hashes, CancellationToken cancellationToken)
        {
            long total = 0;
            var count = 0;
            foreach (var hash in hashes)
            {
                var path = DecodeHash(hash);
                var file = await _fileStore.GetFileInfoAsync(path, cancellationToken);
                if (file != null)
                {
                    total += file.Length;
                    count++;
                }
            }

            return new { size = total, files = count };
        }

        private async Task<Dictionary<string, object>> BuildDirectoryAsync(string path, CancellationToken cancellationToken)
        {
            var entry = await _fileStore.GetDirectoryInfoAsync(path, cancellationToken)
                ?? new ElFinderDirectoryEntry(path);
            return BuildEntryCoreAsync(entry, string.Empty, isCwd: true);
        }

        private Task<Dictionary<string, object>> BuildEntryAsync(IFileStoreEntry entry, string parentDirectoryPath, CancellationToken cancellationToken)
        {
            return Task.FromResult(BuildEntryCoreAsync(entry, parentDirectoryPath, isCwd: false));
        }

        private async Task AppendDirectoryTreeAsync(List<object> files, string path, CancellationToken cancellationToken)
        {
            var children = await _fileStore.GetDirectoryContentAsync(path, includeSubDirectories: false, cancellationToken);
            foreach (var child in children.Where(e => e.IsDirectory).OrderBy(e => e.Name, StringComparer.OrdinalIgnoreCase))
            {
                files.Add(await BuildEntryAsync(child, path, cancellationToken));
                await AppendDirectoryTreeAsync(files, child.Path, cancellationToken);
            }
        }

        private Dictionary<string, object> BuildEntryCoreAsync(IFileStoreEntry entry, string parentDirectoryPath, bool isCwd)
        {
            var path = entry.Path ?? string.Empty;
            var isRoot = isCwd && string.IsNullOrEmpty(path);
            var canWrite = _fileStore.Supports(FileStoreCapabilityFlags.Write);
            var item = new Dictionary<string, object>
            {
                ["name"] = isRoot ? "Home" : entry.Name,
                ["hash"] = EncodeHash(path),
                ["phash"] = isCwd
                    ? (string.IsNullOrEmpty(GetParentPath(path)) ? string.Empty : EncodeHash(GetParentPath(path)))
                    : EncodeHash(parentDirectoryPath ?? string.Empty),
                ["mime"] = entry.IsDirectory ? "directory" : GetMimeType(entry.Name),
                ["ts"] = entry.LastModifiedUtc.ToUnixTimeSeconds(),
                ["date"] = entry.LastModifiedUtc.ToLocalTime().ToString("g", CultureInfo.CurrentCulture),
                ["read"] = 1,
                ["write"] = canWrite ? 1 : 0,
                ["locked"] = 0
            };

            if (entry.IsDirectory)
            {
                item["dirs"] = 1;
                item["size"] = 0;
                item["volumeid"] = VolumeId + "_";
                if (isRoot)
                    item["isroot"] = 1;
            }
            else
            {
                item["size"] = entry.Length;
            }

            return item;
        }

        private static object Error(string message) => new { error = new[] { message } };

        private static string GetParentPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;
            var slash = path.LastIndexOf('/');
            return slash < 0 ? string.Empty : path.Substring(0, slash);
        }

        private static string GetMimeType(string name)
        {
            var extension = Path.GetExtension(name)?.ToLowerInvariant();
            switch (extension)
            {
                case ".txt": return "text/plain";
                case ".html":
                case ".htm": return "text/html";
                case ".json": return "application/json";
                case ".png": return "image/png";
                case ".jpg":
                case ".jpeg": return "image/jpeg";
                case ".gif": return "image/gif";
                case ".pdf": return "application/pdf";
                case ".zip": return "application/zip";
                default: return "application/octet-stream";
            }
        }

        private static IEnumerable<string> ParseHashes(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return Array.Empty<string>();
            return value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        }

        private static string EncodeHash(string path)
        {
            var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(path ?? string.Empty))
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
            return VolumeId + "_" + encoded;
        }

        private static string DecodeHash(string hash)
        {
            if (string.IsNullOrWhiteSpace(hash))
                return string.Empty;
            if (!hash.StartsWith(VolumeId + "_", StringComparison.Ordinal))
                return string.Empty;

            var encoded = hash.Substring(VolumeId.Length + 1).Replace('-', '+').Replace('_', '/');
            switch (encoded.Length % 4)
            {
                case 2: encoded += "=="; break;
                case 3: encoded += "="; break;
            }

            return Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
        }

        private sealed class ElFinderDirectoryEntry : IFileStoreEntry
        {
            public ElFinderDirectoryEntry(string path)
            {
                Path = path ?? string.Empty;
                Name = string.IsNullOrEmpty(Path) ? "Root" : Path.Split('/').Last();
                DirectoryPath = GetParentPath(Path);
                Length = 0;
                LastModifiedUtc = DateTimeOffset.UtcNow;
                IsDirectory = true;
            }

            public string Path { get; }
            public string Name { get; }
            public string DirectoryPath { get; }
            public long Length { get; }
            public DateTimeOffset LastModifiedUtc { get; }
            public bool IsDirectory { get; }
        }
    }

    internal sealed class ElFinderFileDownload
    {
        public ElFinderFileDownload(string path, Stream stream)
        {
            Path = path;
            Stream = stream;
        }

        public string Path { get; }
        public Stream Stream { get; }
    }
}

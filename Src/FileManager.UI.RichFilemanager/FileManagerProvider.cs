using FileManager.FileStorage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#if !NETFULL
using Microsoft.AspNetCore.Http;
#endif

namespace FileManager
{
    public class FileManagerProvider : IFileManagerProvider
    {
        private static readonly string[] DefaultAllowedExtensions =
        {
            "jpg", "jpe", "jpeg", "gif", "png", "svg", "txt", "pdf", "odp", "ods", "odt", "rtf",
            "doc", "docx", "xls", "xlsx", "ppt", "pptx", "csv", "ogv", "avi", "mkv", "mp4", "webm",
            "m4v", "ogg", "mp3", "wav", "zip", "rar", "md"
        };

        private const int SummarizeMaxEntries = 10000;

        private readonly IFileStore _fileStore;
        private readonly DefaultPermissionHandle _permissions;
        private readonly string[] _allowedExtensions;

        public FileManagerProvider(
            IFileStore fileStore,
            DefaultPermissionHandle permissions = null,
            string[] allowedExtensions = null)
        {
            _fileStore = fileStore ?? throw new ArgumentNullException(nameof(fileStore));
            _permissions = permissions;
            _allowedExtensions = allowedExtensions ?? DefaultAllowedExtensions;
        }

        public async Task<Result<ResultInfo>> InitiateAsync(CancellationToken cancellationToken = default)
        {
            var writable = _fileStore.Supports(FileStoreCapabilityFlags.Write)
                && await HasPermissionAsync(null, Permission.Upload, cancellationToken);
            return new Result<ResultInfo>
            {
                Data = new ResultInfo
                {
                    Type = InfoType.initiate,
                    Attributes = new Attributes
                    {
                        Config = new Config
                        {
                            Security = new Security
                            {
                                ReadOnly = !writable,
                                Extensions = new Extensions
                                {
                                    IgnoreCase = true,
                                    Policy = Policy.ALLOW_LIST,
                                    Restrictions = _allowedExtensions
                                }
                            }
                        }
                    }
                }
            };
        }

        public async Task<Result<ResultInfo[]>> SeekFolderAsync(string path, string search, CancellationToken cancellationToken = default)
        {
            path = Normalize(path);
            await EnsureListPermissionAsync(path, cancellationToken);
            var entries = await _fileStore.GetDirectoryContentAsync(path, includeSubDirectories: true, cancellationToken);
            var filtered = entries.Where(entry => string.IsNullOrEmpty(search)
                || entry.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0);

            var results = new List<ResultInfo>();
            foreach (var entry in filtered)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var result = await ToResultInfoAsync(entry, entry.Path, cancellationToken);
                if (result != null)
                    results.Add(result);
            }

            return new Result<ResultInfo[]> { Data = results.ToArray() };
        }

        public async Task<Result<ResultInfo>> GetInfoAsync(string path, CancellationToken cancellationToken = default)
        {
            path = Normalize(path);
            var entry = await GetEntryAsync(path, cancellationToken);
            if (entry == null)
                return NotFound<ResultInfo>(path);

            var result = await ToResultInfoAsync(entry, path, cancellationToken);
            return result == null ? Forbidden<ResultInfo>() : new Result<ResultInfo> { Data = result };
        }

        public async Task<Result<ResultInfo[]>> ReadFolderAsync(string path, CancellationToken cancellationToken = default)
        {
            path = Normalize(path);
            await EnsureListPermissionAsync(path, cancellationToken);
            var entries = await _fileStore.GetDirectoryContentAsync(path, cancellationToken: cancellationToken);
            var results = new List<ResultInfo>();
            foreach (var entry in entries)
            {
                var result = await ToResultInfoAsync(entry, Combine(path, entry.Name), cancellationToken);
                if (result != null)
                    results.Add(result);
            }

            return new Result<ResultInfo[]> { Data = results.ToArray() };
        }

        public async Task<Result<ResultInfo>> AddFolderAsync(string path, string name, CancellationToken cancellationToken = default)
        {
            path = Normalize(path);
            name = VirtualPath.ValidateFileName(name);
            if (!await HasPermissionAsync(await GetDirectoryEntryAsync(path, cancellationToken), Permission.Create, cancellationToken))
                return Forbidden<ResultInfo>();

            var newPath = Combine(path, name);
            if (!await _fileStore.TryCreateDirectoryAsync(newPath, cancellationToken))
                return DirectoryExistsError<ResultInfo>(name);

            return new Result<ResultInfo>
            {
                Data = new ResultInfo
                {
                    Id = MakeWebPath(newPath, isDirectory: true),
                    Type = InfoType.folder,
                    Attributes = new Attributes
                    {
                        Name = name,
                        Path = MakeWebPath(newPath, isDirectory: true),
                        Readable = true,
                        Writable = true,
                        Created = DateTime.UtcNow,
                        Modified = DateTime.UtcNow
                    }
                }
            };
        }

#if !NETFULL
        public async Task<Result<List<ResultInfo>>> UploadAsync(string path, IEnumerable<IFormFile> files, CancellationToken cancellationToken = default)
        {
            var result = new Result<List<ResultInfo>> { Data = new List<ResultInfo>() };
            path = Normalize(path);
            if (!await HasPermissionAsync(await GetDirectoryEntryAsync(path, cancellationToken), Permission.Upload, cancellationToken))
                return Forbidden<List<ResultInfo>>();

            foreach (var file in files ?? Enumerable.Empty<IFormFile>())
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (file.Length <= 0)
                    continue;

                var safeFileName = VirtualPath.ValidateFileName(file.FileName);
                if (!VirtualPath.IsExtensionAllowed(safeFileName, _allowedExtensions))
                    return ExtensionNotAllowedError<List<ResultInfo>>(safeFileName);

                var filePath = Combine(path, safeFileName);
                if (await GetEntryAsync(filePath, cancellationToken) != null)
                    return FileExistsError<List<ResultInfo>>(safeFileName);

                await using (var stream = file.OpenReadStream())
                {
                    await _fileStore.CreateFileFromStreamAsync(filePath, stream, cancellationToken: cancellationToken);
                }

                result.Data.Add(new ResultInfo
                {
                    Id = MakeWebPath(filePath),
                    Type = InfoType.file,
                    Attributes = new Attributes
                    {
                        Name = safeFileName,
                        Extension = Path.GetExtension(safeFileName).TrimStart('.'),
                        Path = MakeWebPath(filePath),
                        Readable = true,
                        Writable = true,
                        Created = DateTime.UtcNow,
                        Modified = DateTime.UtcNow,
                        Size = file.Length
                    }
                });
            }

            return result;
        }
#endif

        public Task<Result<ResultInfo>> RenameAsync(string old, string @new, CancellationToken cancellationToken = default)
        {
            return MoveAsync(old, @new, cancellationToken);
        }

        public async Task<Result<ResultInfo>> MoveAsync(string old, string @new, CancellationToken cancellationToken = default)
        {
            old = Normalize(old);
            @new = Normalize(@new);
            var entry = await GetEntryAsync(old, cancellationToken);
            if (entry == null)
                return NotFound<ResultInfo>(old);

            if (!await HasPermissionAsync(entry, Permission.Rename | Permission.Cut, cancellationToken))
                return Forbidden<ResultInfo>();

            var targetParent = GetParentPath(@new);
            if (!await HasPermissionAsync(await GetDirectoryEntryAsync(targetParent, cancellationToken), Permission.Upload | Permission.Create, cancellationToken))
                return Forbidden<ResultInfo>();

            if (await GetEntryAsync(@new, cancellationToken) != null)
                return entry.IsDirectory ? DirectoryExistsError<ResultInfo>(@new) : FileExistsError<ResultInfo>(@new);

            if (entry.IsDirectory)
                await _fileStore.MoveDirectoryAsync(old, @new, cancellationToken);
            else
                await _fileStore.MoveFileAsync(old, @new, cancellationToken);

            return new Result<ResultInfo>
            {
                Data = new ResultInfo
                {
                    Id = MakeWebPath(@new, entry.IsDirectory),
                    Type = entry.IsDirectory ? InfoType.folder : InfoType.file,
                    Attributes = new Attributes
                    {
                        Name = Path.GetFileName(@new),
                        Path = MakeWebPath(@new, entry.IsDirectory),
                        Readable = true,
                        Writable = true,
                        Modified = DateTime.UtcNow
                    }
                }
            };
        }

        public async Task<Result<ResultInfo>> CopyAsync(string source, string target, CancellationToken cancellationToken = default)
        {
            source = Normalize(source);
            target = Normalize(target);
            var entry = await GetEntryAsync(source, cancellationToken);
            if (entry == null || entry.IsDirectory)
                return NotFound<ResultInfo>(source);

            if (!await HasPermissionAsync(entry, Permission.Copy, cancellationToken))
                return Forbidden<ResultInfo>();

            if (!await HasPermissionAsync(await GetDirectoryEntryAsync(GetParentPath(target), cancellationToken), Permission.Upload | Permission.Create, cancellationToken))
                return Forbidden<ResultInfo>();

            if (await GetEntryAsync(target, cancellationToken) != null)
                return FileExistsError<ResultInfo>(target);

            await _fileStore.CopyFileAsync(source, target, cancellationToken);
            return new Result<ResultInfo>
            {
                Data = new ResultInfo
                {
                    Id = MakeWebPath(target),
                    Type = InfoType.file,
                    Attributes = new Attributes
                    {
                        Name = Path.GetFileName(target),
                        Path = MakeWebPath(target),
                        Readable = true,
                        Writable = true,
                        Modified = DateTime.UtcNow
                    }
                }
            };
        }

        public async Task<Result<ResultInfo>> SaveFileAsync(string path, string content, CancellationToken cancellationToken = default)
        {
            path = Normalize(path);
            if (!VirtualPath.IsExtensionAllowed(path, _allowedExtensions))
                return ExtensionNotAllowedError<ResultInfo>(path);

            var entry = await GetEntryAsync(path, cancellationToken);
            if (!await HasPermissionAsync(entry ?? await GetDirectoryEntryAsync(GetParentPath(path), cancellationToken), Permission.Edit | Permission.Upload, cancellationToken))
                return Forbidden<ResultInfo>();

            using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content ?? string.Empty)))
            {
                await _fileStore.CreateFileFromStreamAsync(path, stream, overwrite: true, cancellationToken: cancellationToken);
            }

            return new Result<ResultInfo>
            {
                Data = new ResultInfo
                {
                    Id = MakeWebPath(path),
                    Type = InfoType.file,
                    Attributes = new Attributes
                    {
                        Name = Path.GetFileName(path),
                        Path = MakeWebPath(path),
                        Readable = true,
                        Writable = true,
                        Modified = DateTime.UtcNow
                    }
                }
            };
        }

        public async Task<Result<ResultInfo>> DeleteAsync(string path, CancellationToken cancellationToken = default)
        {
            path = Normalize(path);
            var entry = await GetEntryAsync(path, cancellationToken);
            if (entry == null)
                return NotFound<ResultInfo>(path);

            if (!await HasPermissionAsync(entry, Permission.Delete, cancellationToken))
                return Forbidden<ResultInfo>();

            if (entry.IsDirectory)
                await _fileStore.TryDeleteDirectoryAsync(path, cancellationToken);
            else
                await _fileStore.TryDeleteFileAsync(path, cancellationToken);

            return new Result<ResultInfo>
            {
                Data = new ResultInfo
                {
                    Id = MakeWebPath(path, entry.IsDirectory),
                    Type = entry.IsDirectory ? InfoType.folder : InfoType.file,
                    Attributes = new Attributes
                    {
                        Name = entry.Name,
                        Path = MakeWebPath(path, entry.IsDirectory),
                        Readable = true,
                        Writable = true,
                        Modified = DateTime.UtcNow
                    }
                }
            };
        }

        public async Task<Stream> OpenReadAsync(string path, CancellationToken cancellationToken = default)
        {
            path = Normalize(path);
            var entry = await GetEntryAsync(path, cancellationToken);
            if (entry == null || entry.IsDirectory)
                throw new FileStoreException($"The file '{path}' does not exist.");

            if (!await HasPermissionAsync(entry, Permission.Download | Permission.Preview, cancellationToken))
                throw new FileStoreException("Read access denied.");

            return await _fileStore.GetFileStreamAsync(path, cancellationToken);
        }

        public async Task<Result<ResultInfo>> SummarizeAsync(CancellationToken cancellationToken = default)
        {
            var entries = (await _fileStore.GetDirectoryContentAsync(includeSubDirectories: true, cancellationToken: cancellationToken))
                .Take(SummarizeMaxEntries)
                .ToArray();
            var files = entries.Where(entry => !entry.IsDirectory).ToArray();
            var folders = entries.Where(entry => entry.IsDirectory).ToArray();

            return new Result<ResultInfo>
            {
                Data = new ResultInfo
                {
                    Id = "/",
                    Type = InfoType.summary,
                    Attributes = new Attributes
                    {
                        Size = files.Sum(file => file.Length),
                        Files = files.Length,
                        Folders = folders.Length,
                        SizeLimit = SummarizeMaxEntries
                    }
                }
            };
        }

        private async Task<IFileStoreEntry> GetEntryAsync(string path, CancellationToken cancellationToken)
        {
            return await _fileStore.GetFileInfoAsync(path, cancellationToken)
                ?? await _fileStore.GetDirectoryInfoAsync(path, cancellationToken);
        }

        private Task<IFileStoreEntry> GetDirectoryEntryAsync(string path, CancellationToken cancellationToken)
        {
            return _fileStore.GetDirectoryInfoAsync(path, cancellationToken);
        }

        private async Task<bool> HasPermissionAsync(IFileStoreEntry entry, Permission required, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_permissions == null)
            {
                var writeFlags = Permission.Create | Permission.Delete | Permission.Rename | Permission.Upload
                    | Permission.Edit | Permission.Cut | Permission.Copy | Permission.Paste | Permission.Compress | Permission.Extract;
                if ((required & writeFlags) != 0)
                    return false;
                return true;
            }

            if (entry == null)
                return (required & (Permission.ListFiles | Permission.ListSubFolders)) != 0;

            var granted = await _permissions.GetPermissionAsync(entry);
            return (granted & required) != 0;
        }

        private async Task EnsureListPermissionAsync(string path, CancellationToken cancellationToken)
        {
            var directory = await GetDirectoryEntryAsync(path, cancellationToken);
            if (directory != null && !await HasPermissionAsync(directory, Permission.ListFiles | Permission.ListSubFolders, cancellationToken))
                throw new FileStoreException("List access denied.");
        }

        private async Task<ResultInfo> ToResultInfoAsync(IFileStoreEntry entry, string relativePath, CancellationToken cancellationToken)
        {
            relativePath = Normalize(relativePath);
            var required = entry.IsDirectory
                ? Permission.ListSubFolders
                : Permission.Download | Permission.Preview;
            if (!await HasPermissionAsync(entry, required, cancellationToken))
                return null;

            var writable = _fileStore.Supports(FileStoreCapabilityFlags.Write)
                && await HasPermissionAsync(entry, Permission.Upload | Permission.Edit | Permission.Delete | Permission.Rename, cancellationToken);

            return new ResultInfo
            {
                Id = MakeWebPath(relativePath, entry.IsDirectory),
                Type = entry.IsDirectory ? InfoType.folder : InfoType.file,
                Attributes = new Attributes
                {
                    Name = entry.Name,
                    Path = MakeWebPath(relativePath, entry.IsDirectory),
                    Readable = true,
                    Writable = writable,
                    Modified = entry.LastModifiedUtc.UtcDateTime,
                    Extension = entry.IsDirectory ? null : Path.GetExtension(entry.Name).TrimStart('.'),
                    Size = entry.IsDirectory ? (long?)null : entry.Length
                }
            };
        }

        private static string Normalize(string path)
        {
            return string.IsNullOrWhiteSpace(path) ? string.Empty : VirtualPath.Normalize(path);
        }

        private static string Combine(string path, string name)
        {
            path = Normalize(path);
            name = VirtualPath.ValidateFileName(name);
            if (string.IsNullOrEmpty(path))
                return name;
            return path + "/" + name;
        }

        private static string GetParentPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;

            var index = path.LastIndexOf('/');
            return index < 0 ? string.Empty : path.Substring(0, index);
        }

        private static string MakeWebPath(string path, bool isDirectory = false)
        {
            path = "/" + Normalize(path);
            if (isDirectory && !path.EndsWith("/", StringComparison.Ordinal))
                path += "/";
            return path;
        }

        private static Result<TResult> NotFound<TResult>(string path)
        {
            return new Result<TResult>
            {
                Errors = new List<Error>
                {
                    new Error { Code = 404, Title = "NOT_FOUND", Meta = new Meta { Arguments = new List<string> { path } } }
                }
            };
        }

        private static Result<TResult> Forbidden<TResult>()
        {
            return new Result<TResult>
            {
                Errors = new List<Error>
                {
                    new Error { Code = 403, Title = "FORBIDDEN", Meta = new Meta { Arguments = new List<string>() } }
                }
            };
        }

        private static Result<TResult> DirectoryExistsError<TResult>(string name)
        {
            return new Result<TResult>
            {
                Errors = new List<Error>
                {
                    new Error { Code = 500, Title = "DIRECTORY_ALREADY_EXISTS", Meta = new Meta { Arguments = new List<string> { name } } }
                }
            };
        }

        private static Result<TResult> FileExistsError<TResult>(string name)
        {
            return new Result<TResult>
            {
                Errors = new List<Error>
                {
                    new Error { Code = 500, Title = "FILE_ALREADY_EXISTS", Meta = new Meta { Arguments = new List<string> { name } } }
                }
            };
        }

        private static Result<TResult> ExtensionNotAllowedError<TResult>(string name)
        {
            return new Result<TResult>
            {
                Errors = new List<Error>
                {
                    new Error { Code = 400, Title = "EXTENSION_NOT_ALLOWED", Meta = new Meta { Arguments = new List<string> { name } } }
                }
            };
        }
    }
}

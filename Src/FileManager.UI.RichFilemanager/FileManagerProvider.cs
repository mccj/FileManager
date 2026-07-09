using FileManager.FileStorage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        private readonly IFileStore _fileStore;
        private readonly string[] _allowedExtensions;

        public FileManagerProvider(IFileStore fileStore, string[] allowedExtensions = null)
        {
            _fileStore = fileStore ?? throw new ArgumentNullException(nameof(fileStore));
            _allowedExtensions = allowedExtensions ?? DefaultAllowedExtensions;
        }

        public Result<ResultInfo> Initiate()
        {
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
                                ReadOnly = false,
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

        public Result<ResultInfo[]> SeekFolder(string path, string search)
        {
            path = Normalize(path);
            var entries = _fileStore.GetDirectoryContentAsync(path, includeSubDirectories: true)
                .GetAwaiter().GetResult()
                .Where(entry => string.IsNullOrEmpty(search)
                    || entry.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0);

            return new Result<ResultInfo[]>
            {
                Data = entries.Select(entry => ToResultInfo(entry, entry.Path)).ToArray()
            };
        }

        public Result<ResultInfo> GetInfo(string path)
        {
            path = Normalize(path);
            var entry = GetEntry(path);
            if (entry == null)
            {
                return new Result<ResultInfo>
                {
                    Errors = new List<Error>
                    {
                        new Error { Code = 404, Title = "NOT_FOUND", Meta = new Meta { Arguments = new List<string> { path } } }
                    }
                };
            }

            return new Result<ResultInfo> { Data = ToResultInfo(entry, path) };
        }

        public Result<ResultInfo[]> ReadFolder(string path)
        {
            path = Normalize(path);
            var entries = _fileStore.GetDirectoryContentAsync(path).GetAwaiter().GetResult();
            return new Result<ResultInfo[]>
            {
                Data = entries.Select(entry => ToResultInfo(entry, Combine(path, entry.Name))).ToArray()
            };
        }

        public Result<ResultInfo> AddFolder(string path, string name)
        {
            var newPath = Combine(path, name);
            if (!_fileStore.TryCreateDirectoryAsync(newPath).GetAwaiter().GetResult())
            {
                return DirectoryExistsError<ResultInfo>(name);
            }

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
        public Task<Result<List<ResultInfo>>> Upload(string path, IEnumerable<IFormFile> files)
        {
            return Task.Run(() =>
            {
                var result = new Result<List<ResultInfo>> { Data = new List<ResultInfo>() };
                path = Normalize(path);

                foreach (var file in files ?? Enumerable.Empty<IFormFile>())
                {
                    if (file.Length <= 0)
                        continue;

                    var filePath = Combine(path, file.FileName);
                    if (GetEntry(filePath) != null)
                    {
                        return FileExistsError<List<ResultInfo>>(file.FileName);
                    }

                    using (var stream = file.OpenReadStream())
                    {
                        _fileStore.CreateFileFromStreamAsync(filePath, stream).GetAwaiter().GetResult();
                    }

                    result.Data.Add(new ResultInfo
                    {
                        Id = MakeWebPath(filePath),
                        Type = InfoType.file,
                        Attributes = new Attributes
                        {
                            Name = file.FileName,
                            Extension = Path.GetExtension(file.FileName).TrimStart('.'),
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
            });
        }
#endif

        public Result<ResultInfo> Rename(string old, string @new)
        {
            return Move(old, @new);
        }

        public Result<ResultInfo> Move(string old, string @new)
        {
            old = Normalize(old);
            @new = Normalize(@new);
            var entry = GetEntry(old);
            if (entry == null)
            {
                return new Result<ResultInfo>
                {
                    Errors = new List<Error>
                    {
                        new Error { Code = 404, Title = "NOT_FOUND", Meta = new Meta { Arguments = new List<string> { old } } }
                    }
                };
            }

            if (GetEntry(@new) != null)
            {
                return entry.IsDirectory ? DirectoryExistsError<ResultInfo>(@new) : FileExistsError<ResultInfo>(@new);
            }

            if (entry.IsDirectory)
                _fileStore.MoveDirectoryAsync(old, @new).GetAwaiter().GetResult();
            else
                _fileStore.MoveFileAsync(old, @new).GetAwaiter().GetResult();

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

        public Result<ResultInfo> Copy(string source, string target)
        {
            source = Normalize(source);
            target = Normalize(target);
            var entry = GetEntry(source);
            if (entry == null || entry.IsDirectory)
            {
                return new Result<ResultInfo>
                {
                    Errors = new List<Error>
                    {
                        new Error { Code = 404, Title = "NOT_FOUND", Meta = new Meta { Arguments = new List<string> { source } } }
                    }
                };
            }

            if (GetEntry(target) != null)
                return FileExistsError<ResultInfo>(target);

            _fileStore.CopyFileAsync(source, target).GetAwaiter().GetResult();
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

        public Result<ResultInfo> SaveFile(string path, string content)
        {
            path = Normalize(path);
            using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content ?? string.Empty)))
            {
                _fileStore.CreateFileFromStreamAsync(path, stream, overwrite: true).GetAwaiter().GetResult();
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

        public Result<ResultInfo> Delete(string path)
        {
            path = Normalize(path);
            var entry = GetEntry(path);
            if (entry == null)
            {
                return new Result<ResultInfo>
                {
                    Errors = new List<Error>
                    {
                        new Error { Code = 404, Title = "NOT_FOUND", Meta = new Meta { Arguments = new List<string> { path } } }
                    }
                };
            }

            if (entry.IsDirectory)
                _fileStore.TryDeleteDirectoryAsync(path).GetAwaiter().GetResult();
            else
                _fileStore.TryDeleteFileAsync(path).GetAwaiter().GetResult();

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

        public byte[] ReadFile(string path)
        {
            path = Normalize(path);
            using (var stream = _fileStore.GetFileStreamAsync(path).GetAwaiter().GetResult())
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        public byte[] GetImage(string path, bool thumbnail)
        {
            return ReadFile(path);
        }

        public Result<ResultInfo> Summarize()
        {
            var entries = _fileStore.GetDirectoryContentAsync(includeSubDirectories: true).GetAwaiter().GetResult().ToArray();
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
                        SizeLimit = 0
                    }
                }
            };
        }

        private IFileStoreEntry GetEntry(string path)
        {
            return _fileStore.GetFileInfoAsync(path).GetAwaiter().GetResult()
                ?? _fileStore.GetDirectoryInfoAsync(path).GetAwaiter().GetResult();
        }

        private static string Normalize(string path)
        {
            return string.IsNullOrWhiteSpace(path) ? string.Empty : path.Trim('/').Replace('\\', '/');
        }

        private static string Combine(string path, string name)
        {
            path = Normalize(path);
            name = Normalize(name);
            if (string.IsNullOrEmpty(path))
                return name;
            if (string.IsNullOrEmpty(name))
                return path;
            return path + "/" + name;
        }

        private static string MakeWebPath(string path, bool isDirectory = false)
        {
            path = "/" + Normalize(path);
            if (isDirectory && !path.EndsWith("/", StringComparison.Ordinal))
                path += "/";
            return path;
        }

        private static ResultInfo ToResultInfo(IFileStoreEntry entry, string relativePath)
        {
            relativePath = Normalize(relativePath);
            return new ResultInfo
            {
                Id = MakeWebPath(relativePath, entry.IsDirectory),
                Type = entry.IsDirectory ? InfoType.folder : InfoType.file,
                Attributes = new Attributes
                {
                    Name = entry.Name,
                    Path = MakeWebPath(relativePath, entry.IsDirectory),
                    Readable = true,
                    Writable = true,
                    Modified = entry.LastModifiedUtc.UtcDateTime,
                    Extension = entry.IsDirectory ? null : Path.GetExtension(entry.Name).TrimStart('.'),
                    Size = entry.IsDirectory ? (long?)null : entry.Length
                }
            };
        }

        private static Result<TResult> DirectoryExistsError<TResult>(string name)
        {
            return new Result<TResult>
            {
                Errors = new List<Error>
                {
                    new Error
                    {
                        Code = 500,
                        Title = "DIRECTORY_ALREADY_EXISTS",
                        Meta = new Meta { Arguments = new List<string> { name } }
                    }
                }
            };
        }

        private static Result<TResult> FileExistsError<TResult>(string name)
        {
            return new Result<TResult>
            {
                Errors = new List<Error>
                {
                    new Error
                    {
                        Code = 500,
                        Title = "FILE_ALREADY_EXISTS",
                        Meta = new Meta { Arguments = new List<string> { name } }
                    }
                }
            };
        }
    }
}

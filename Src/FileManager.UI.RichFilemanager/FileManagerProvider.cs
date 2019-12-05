////using Microsoft.AspNetCore.Http;
//using SixLabors.ImageSharp;
//using SixLabors.ImageSharp.Processing;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Threading.Tasks;
//using FileManager.FileStorage;
//#if !NETFULL
//using Microsoft.AspNetCore.StaticFiles;
//#else
//#endif
//namespace FileManager
//{
//    public class FileManagerProvider : IFileManagerProvider
//    {
//        //private readonly FileExtensionContentTypeProvider fileExtensionContentTypeProvider = new FileExtensionContentTypeProvider();

//        private readonly IFileStore _fileStore;
//        private readonly string _webRootPath;
//        private readonly string _webPath;
//        private readonly bool _publicPath;
//        private readonly string[] _allowedExtensions;
//        //private string[] imgExtensions = new string[] { ".jpg", ".png", ".jpeg", ".gif", ".bmp" }; // Only allow this image extensions. [string]

//        public FileManagerProvider(IFileStore fileStore, string webRootPath, string webPath = null, bool publicPath = false, string[] allowedExtensions = null)
//        {
//            _fileStore = fileStore;
//            // FileManager Content Folder
//            _webPath = webPath;
//            _publicPath = publicPath;
//            //if (string.IsNullOrWhiteSpace(env.WebRootPath))
//            //{
//            //    env.WebRootPath = Directory.GetCurrentDirectory();
//            //}
//            //_webRootPath = Path.Combine(env.WebRootPath, _webPath);
//            _webRootPath = Path.Combine(webRootPath, _webPath);
//            _allowedExtensions = allowedExtensions ?? new[] { "jpg", "jpe", "jpeg", "gif", "png", "svg", "txt", "pdf", "odp", "ods", "odt", "rtf", "doc", "docx", "xls", "xlsx", "ppt", "pptx", "csv", "ogv", "avi", "mkv", "mp4", "webm", "m4v", "ogg", "mp3", "wav", "zip", "rar", "md" };
//        }

//        public Result<ResultInfo> Initiate()
//        {
//            var result = new Result<ResultInfo>
//            {
//                Data = new ResultInfo
//                {
//                    Type = InfoType.initiate,
//                    Attributes = new Attributes
//                    {
//                        Config = new Config
//                        {
//                            Security = new Security
//                            {
//                                ReadOnly = false,
//                                Extensions = new Extensions
//                                {
//                                    IgnoreCase = true,
//                                    Policy = Policy.ALLOW_LIST,
//                                    Restrictions = _allowedExtensions
//                                }
//                            }
//                        }
//                    }
//                }
//            };

//            return result;
//        }

//        public Result<ResultInfo[]> SeekFolder(string path, string search)
//        {
//            //if (path == null) { path = string.Empty; };

//            //var searchPath = Path.Combine(_webRootPath, path);
//            //var data = new List<ResultInfo>();

//            //foreach (FileInfo file in new DirectoryInfo(searchPath).GetFiles("*" + search + "*", SearchOption.AllDirectories).Where(f => ! «∑ÒÀı¬‘Õº(f.FullName)))
//            //{
//            //    var item = new ResultInfo
//            //    {
//            //        Id = MakeWebPath(Path.Combine(GetRelativePath(_webRootPath, file.DirectoryName), file.Name), true),
//            //        Type = InfoType.file,
//            //        Attributes = new Attributes
//            //        {
//            //            Name = file.Name,
//            //            Path = getPath(() => MakeWebPath(Path.Combine(GetRelativePath(_webRootPath, file.DirectoryName), file.Name), true)),
//            //            Readable = true,
//            //            Writable = true,
//            //            Created = /*GetUnixTimestamp*/(file.CreationTimeUtc),
//            //            Modified = /*GetUnixTimestamp*/(file.LastWriteTimeUtc),
//            //            Size = file.Length,
//            //            Extension = file.Extension.TrimStart('.'),
//            //            // Insert Height and Width logic for images
//            //            Timestamp = DateTime.Now.Subtract(file.LastWriteTime)/*.TotalSeconds*/
//            //        }
//            //    };
//            //    data.Add(item);
//            //}
//            //foreach (DirectoryInfo dir in new DirectoryInfo(searchPath).GetDirectories("*" + search + "*", SearchOption.AllDirectories))
//            //{

//            //    var item = new ResultInfo
//            //    {
//            //        Id = MakeWebPath(GetRelativePath(_webRootPath, dir.FullName), false, true),
//            //        Type = InfoType.folder,
//            //        Attributes = new Attributes
//            //        {
//            //            Name = dir.Name,
//            //            Path = getPath(() => MakeWebPath(dir.FullName, true, true)),
//            //            Readable = true,
//            //            Writable = true,
//            //            Created = /*GetUnixTimestamp*/(dir.CreationTimeUtc),
//            //            Modified = /*GetUnixTimestamp*/(dir.LastWriteTimeUtc),
//            //            Timestamp = DateTime.Now.Subtract(dir.LastWriteTime)/*.TotalSeconds*/
//            //        }
//            //    };
//            //    data.Add(item);
//            //}
//            //return new Result<ResultInfo[]>
//            //{
//            //    Data = data.ToArray()
//            //};
//            return null;
//        }

//        public Result<ResultInfo> GetInfo(string path)
//        {
//            if (path == null) { path = string.Empty; };

//            var filePath = Path.Combine(_webRootPath, path);
//            var file = _fileStore.GetFileInfoAsync(filePath).Result;

//            return new Result<ResultInfo>
//            {
//                Data = new ResultInfo
//                {
//                    Id = MakeWebPath(Path.Combine(GetRelativePath(_webRootPath, Path.GetDirectoryName(file.Path)), file.Name), true),
//                    Type = InfoType.file,
//                    Attributes = new Attributes
//                    {
//                        Name = file.Name,
//                        Path = getPath(() => MakeWebPath(Path.Combine(GetRelativePath(_webRootPath, Path.GetDirectoryName(file.Path)), file.Name), false)),
//                        Readable = true,
//                        Writable = true,
//                        //Created = /*GetUnixTimestamp*/(file.CreationTimeUtc),
//                        Modified = /*GetUnixTimestamp*/(file.LastModifiedUtc),
//                        Size = file.Length,
//                        Extension = Path.GetExtension(file.Name).Replace(".", "")//,
//                        //Timestamp = DateTime.Now.Subtract(file.LastModifiedTimeUtc)/*.TotalSeconds*/
//                    }
//                }
//            };
//        }

//        public Result<ResultInfo[]> ReadFolder(string path)
//        {
//            if (path == null) path = string.Empty;

//            var rootpath = Path.Combine(_webRootPath, path);

//            var rootDirectory = _fileStore.GetDirectoryContentAsync(rootpath).Result;

//            var data = rootDirectory.Where(f => ! «∑ÒÀı¬‘Õº(f))
//                .Select(file => new ResultInfo
//                {
//                    Id = MakeWebPath(Path.Combine(path, file.Name), false, file.IsDirectory),
//                    Type = file.IsDirectory ? InfoType.folder : InfoType.file,
//                    Attributes = new Attributes
//                    {
//                        Name = file.Name,
//                        Path = getPath(() => MakeWebPath(Path.Combine(_webPath, path, file.Name), true, file.IsDirectory)),
//                        Readable = true,
//                        Writable = true,
//                        //Created = /*GetUnixTimestamp*/(file.CreationTimeUtc),
//                        Modified = /*GetUnixTimestamp*/(file.LastModifiedUtc),
//                        Extension = Path.GetExtension(file.Name).Replace(".", ""),
//                        Size = file.Length//,
//                                          //Timestamp = DateTime.Now.Subtract(file.LastModifiedTimeUtc)/*.TotalSeconds*/
//                                          //Height = 10,
//                                          //Width = 10  }
//                    }
//                }).ToArray();

//            var result = new Result<ResultInfo[]>
//            {
//                Data = data.ToArray()
//            };
//            return result;
//        }

//        public Result<ResultInfo> AddFolder(string path, string name)
//        {
//            var newDirectoryPath = Path.Combine(_webRootPath, path, name);

//            var isok = _fileStore.TryCreateDirectoryAsync(newDirectoryPath).Result;

//            if (!isok)
//            {
//                var errorResult = new Result<ResultInfo> { Errors = new List<Error>() };

//                errorResult.Errors.Add(new Error
//                {
//                    Code = 500,
//                    Title = "DIRECTORY_ALREADY_EXISTS",
//                    Meta = new Meta
//                    {
//                        Arguments = new List<string>
//                        {
//                            name
//                        }
//                    }
//                });

//                return errorResult;
//            }
//            else
//            {
//                var result = new Result<ResultInfo>
//                {
//                    Data =
//                        new ResultInfo
//                        {
//                            Id = MakeWebPath(Path.Combine(path, name), false, true),
//                            Type = InfoType.folder,
//                            Attributes = new Attributes
//                            {
//                                Name = name,
//                                Path = getPath(() => MakeWebPath(Path.Combine(_webPath, path, name), true, true)),
//                                Readable = true,
//                                Writable = true,
//                                Created = /*GetUnixTimestamp*/(DateTime.Now),
//                                Modified = /*GetUnixTimestamp*/(DateTime.Now)
//                            }
//                        }
//                };

//                return result;
//            }
//        }
//#if !NETFULL
//        public Task<Result<List<ResultInfo>>> Upload(string path, IEnumerable<Microsoft.AspNetCore.Http.IFormFile> files)
//        {
//            return Task.Run(() =>
//            {
//                var result = new Result<List<ResultInfo>> { Data = new List<ResultInfo>() };

//                foreach (var file in files)
//                {
//                    if (file.Length <= 0) continue;

//                    var fileExist = _fileStore.FileExists(Path.Combine(_webRootPath, path, file.FileName));

//                    if (fileExist)
//                    {
//                        var errorResult = new Result<List<ResultInfo>> { Errors = new List<Error>() };

//                        errorResult.Errors.Add(new Error
//                        {
//                            Code = 500,
//                            Title = "FILE_ALREADY_EXISTS",
//                            Meta = new Meta
//                            {
//                                Arguments = new List<string>
//                            {
//                                file.FileName
//                            }
//                            }
//                        });

//                        return errorResult;
//                    }

//                    var _path = Path.Combine(_webRootPath, path, file.FileName);
//                    using (var stream = file.OpenReadStream())
//                    {
//                        _fileStore.CreateFileFromStreamAsync(_path, stream).Wait();
//                    }

//                    …æ≥˝Àı¬‘Õº(_path);
//                    result.Data.Add(new ResultInfo
//                    {
//                        Id = MakeWebPath(Path.Combine(path, file.FileName)),
//                        Type = InfoType.file,
//                        Attributes = new Attributes
//                        {
//                            Name = file.FileName,
//                            Extension = Path.GetExtension(file.FileName).Replace(".", ""),
//                            Path = getPath(() => MakeWebPath(Path.Combine(_webPath, path, file.FileName), true)),
//                            Readable = true,
//                            Writable = true,
//                            Created = /*GetUnixTimestamp*/(DateTime.Now),
//                            Modified = /*GetUnixTimestamp*/(DateTime.Now),
//                            Size = file.Length
//                        }
//                    });

//                }

//                return result;
//            });
//        }
//#else
//#endif

//        public Result<ResultInfo> Rename(string old, string @new)
//        {
//            var oldPath = Path.Combine(_webRootPath, old);

//            if (_fileStore.IsDirectory(oldPath).Result) //Fixed if the directory is compressed
//            {
//                var oldDirectoryName = Path.GetDirectoryName(old).Split('\\').Last();
//                var newDirectoryPath = old.Replace(oldDirectoryName, @new);
//                var newPath = Path.Combine(_webRootPath, newDirectoryPath);

//                var directoryExist = _fileStore.DirectoryExists(newPath);

//                if (directoryExist)
//                {
//                    var errorResult = new Result<ResultInfo> { Errors = new List<Error>() };

//                    errorResult.Errors.Add(new Error
//                    {
//                        Code = 500,
//                        Title = "DIRECTORY_ALREADY_EXISTS",
//                        Meta = new Meta
//                        {
//                            Arguments = new List<string>
//                            {
//                                @new
//                            }
//                        }
//                    });

//                    return errorResult;
//                }

//                _fileStore.MoveDirectoryAsync(oldPath, newPath);

//                var result = new Result<ResultInfo>
//                {
//                    Data = new ResultInfo
//                    {
//                        Id = newDirectoryPath,
//                        Type = InfoType.folder,
//                        Attributes = new Attributes
//                        {
//                            Name = @new,
//                            Readable = true,
//                            Writable = true,
//                            Created = /*GetUnixTimestamp*/(DateTime.Now),
//                            Modified = /*GetUnixTimestamp*/(DateTime.Now)
//                        }
//                    }
//                };

//                return result;
//            }
//            else
//            {

//                var oldFileName = Path.GetFileName(old);
//                var newFilePath = old.Replace(oldFileName, @new);
//                var newPath = Path.Combine(_webRootPath, newFilePath);

//                var fileExist = _fileStore.FileExists(newPath);

//                if (fileExist)
//                {
//                    var errorResult = new Result<ResultInfo> { Errors = new List<Error>() };

//                    errorResult.Errors.Add(new Error
//                    {
//                        Code = 500,
//                        Title = "FILE_ALREADY_EXISTS",
//                        Meta = new Meta
//                        {
//                            Arguments = new List<string>
//                            {
//                                @new
//                            }
//                        }
//                    });

//                    return errorResult;
//                }

//                _fileStore.MoveFileAsync(oldPath, newPath);
//                …æ≥˝Àı¬‘Õº(oldPath);
//                …æ≥˝Àı¬‘Õº(newPath);

//                var result = new Result<ResultInfo>
//                {
//                    Data = new ResultInfo
//                    {
//                        Id = newFilePath,
//                        Type = InfoType.file,
//                        Attributes = new Attributes
//                        {
//                            Name = @new,
//                            Extension = Path.GetExtension(newPath).Replace(".", ""),
//                            Readable = true,
//                            Writable = true,
//                            // created date, size vb.
//                            Created = /*GetUnixTimestamp*/(DateTime.Now),
//                            Modified = /*GetUnixTimestamp*/(DateTime.Now)
//                        }
//                    }
//                };

//                return result;
//            }
//        }

//        public Result<ResultInfo> Move(string old, string @new)
//        {
//            var oldPath = Path.Combine(_webRootPath, old);

//            if (_fileStore.IsDirectory(oldPath).Result) //Fixed if the directory is compressed
//            {
//                var directoryName = Path.GetDirectoryName(old).Split('\\').Last();
//                var newDirectoryPath = Path.Combine(@new, directoryName);
//                //var oldPath = Path.Combine(_webRootPath, old);
//                var newPath = Path.Combine(_webRootPath, @new, directoryName);


//                var directoryExist = _fileStore.DirectoryExists(newPath);

//                if (directoryExist)
//                {
//                    var errorResult = new Result<ResultInfo> { Errors = new List<Error>() };

//                    errorResult.Errors.Add(new Error
//                    {
//                        Code = 500,
//                        Title = "DIRECTORY_ALREADY_EXISTS",
//                        Meta = new Meta
//                        {
//                            Arguments = new List<string>
//                            {
//                                directoryName
//                            }
//                        }
//                    });

//                    return errorResult;
//                }
//                _fileStore.MoveDirectoryAsync(oldPath, newPath);

//                var result = new Result<ResultInfo>
//                {
//                    Data = new ResultInfo
//                    {
//                        Id = newDirectoryPath,
//                        Type = InfoType.folder,
//                        Attributes = new Attributes
//                        {
//                            Name = directoryName,
//                            Readable = true,
//                            Writable = true,
//                            Created = /*GetUnixTimestamp*/(DateTime.Now),
//                            Modified = /*GetUnixTimestamp*/(DateTime.Now)
//                        }
//                    }
//                };

//                return result;
//            }
//            else
//            {
//                var fileName = Path.GetFileName(old);
//                var newFilePath = Path.Combine(@new, fileName);
//                //var oldPath = Path.Combine(_webRootPath, old);

//                var newPath = @new == "/"
//                    ? Path.Combine(_webRootPath, fileName.Replace("/", ""))
//                    : Path.Combine(_webRootPath, @new, fileName);


//                var fileExist = _fileStore.FileExists(newPath);

//                if (fileExist)
//                {
//                    var errorResult = new Result<ResultInfo> { Errors = new List<Error>() };

//                    errorResult.Errors.Add(new Error
//                    {
//                        Code = 500,
//                        Title = "FILE_ALREADY_EXISTS",
//                        Meta = new Meta
//                        {
//                            Arguments = new List<string>
//                            {
//                                fileName
//                            }
//                        }
//                    });

//                    return errorResult;
//                }

//                _fileStore.MoveFileAsync(oldPath, newPath);
//                …æ≥˝Àı¬‘Õº(oldPath);
//                …æ≥˝Àı¬‘Õº(newPath);

//                var result = new Result<ResultInfo>
//                {
//                    Data = new ResultInfo
//                    {
//                        Id = newFilePath,
//                        Type = InfoType.file,
//                        Attributes = new Attributes
//                        {
//                            Name = fileName,
//                            Extension = Path.GetExtension(@new).Replace(".", ""),
//                            Readable = true,
//                            Writable = true,
//                            Created = /*GetUnixTimestamp*/(DateTime.Now),
//                            Modified = /*GetUnixTimestamp*/(DateTime.Now)
//                        }
//                    }
//                };

//                return result;
//            }
//        }

//        public Result<ResultInfo> Copy(string source, string target)
//        {
//            var sourcePath = Path.Combine(_webRootPath, source);

//            if (_fileStore.IsDirectory(sourcePath).Result) //Fixed if the directory is compressed
//            {
//                var directoryName = Path.GetDirectoryName(source).Split('\\').Last();
//                var newDirectoryPath = Path.Combine(target, directoryName);
//                var oldPath = Path.Combine(_webRootPath, source);
//                var newPath = Path.Combine(_webRootPath, target, directoryName);


//                var directoryExist = _fileStore.DirectoryExists(newPath);

//                if (directoryExist)
//                {
//                    var errorResult = new Result<ResultInfo> { Errors = new List<Error>() };

//                    errorResult.Errors.Add(new Error
//                    {
//                        Code = 500,
//                        Title = "DIRECTORY_ALREADY_EXISTS",
//                        Meta = new Meta
//                        {
//                            Arguments = new List<string>
//                            {
//                                directoryName
//                            }
//                        }
//                    });

//                    return errorResult;
//                }
//                _fileStore.CopyDirectoryAsync(oldPath, newPath);
//                //DirectoryCopy(oldPath, newPath);

//                var result = new Result<ResultInfo>
//                {
//                    Data = new ResultInfo
//                    {
//                        Id = newDirectoryPath,
//                        Type = InfoType.folder,
//                        Attributes = new Attributes
//                        {
//                            Name = directoryName,
//                            Readable = true,
//                            Writable = true,
//                            Created = /*GetUnixTimestamp*/(DateTime.Now),
//                            Modified = /*GetUnixTimestamp*/(DateTime.Now)
//                        }
//                    }
//                };

//                return result;
//            }
//            else
//            {
//                var fileName = Path.GetFileName(source);
//                var newFilePath = Path.Combine(@target, fileName);
//                var oldPath = Path.Combine(_webRootPath, source);
//                var newPath = Path.Combine(_webRootPath, target, fileName);

//                var fileExist = _fileStore.FileExists(newPath);

//                if (fileExist)
//                {
//                    var errorResult = new Result<ResultInfo> { Errors = new List<Error>() };

//                    errorResult.Errors.Add(new Error
//                    {
//                        Code = 500,
//                        Title = "FILE_ALREADY_EXISTS",
//                        Meta = new Meta
//                        {
//                            Arguments = new List<string>
//                            {
//                                fileName
//                            }
//                        }
//                    });

//                    return errorResult;
//                }

//                _fileStore.CopyFileAsync(oldPath, newPath);
//                …æ≥˝Àı¬‘Õº(newPath);

//                var result = new Result<ResultInfo>
//                {
//                    Data = new ResultInfo
//                    {
//                        Id = newFilePath,
//                        Type = InfoType.file,
//                        Attributes = new Attributes
//                        {
//                            Name = fileName,
//                            Extension = Path.GetExtension(fileName).Replace(".", ""),
//                            Readable = true,
//                            Writable = true,
//                            // created date, size vb.
//                            Created = /*GetUnixTimestamp*/(DateTime.Now),
//                            Modified = /*GetUnixTimestamp*/(DateTime.Now)
//                        }
//                    }
//                };

//                return result;

//            }
//        }

//        public Result<ResultInfo> SaveFile(string path, string content)
//        {
//            var filePath = Path.Combine(_webRootPath, path);
//            using (MemoryStream stream = new MemoryStream())
//            {
//                _fileStore.CreateFileFromStream(filePath, stream);
//                using (StreamWriter streamWriter = new System.IO.StreamWriter(stream))
//                {
//                    streamWriter.Write(content);
//                }
//            }
//            //…æ≥˝Àı¬‘Õº(filePath);

//            var fileName = Path.GetFileName(path);
//            var fileExtension = Path.GetExtension(fileName);

//            var result = new Result<ResultInfo>
//            {
//                Data = new ResultInfo
//                {
//                    Id = path,
//                    Type = InfoType.file,
//                    Attributes = new Attributes
//                    {
//                        Name = fileName,
//                        Extension = fileExtension,
//                        Readable = true,
//                        Writable = true
//                    }
//                }
//            };

//            return result;
//        }

//        public Result<ResultInfo> Delete(string path)
//        {
//            var filePath = Path.Combine(_webRootPath, path);

//            if (_fileStore.IsDirectory(filePath).Result) //Fixed if the directory is compressed
//            {
//                var directoryName = Path.GetDirectoryName(path).Split('\\').Last();
//                _fileStore.TryDeleteDirectoryAsync(filePath).Wait();

//                var result = new Result<ResultInfo>
//                {
//                    Data = new ResultInfo
//                    {
//                        Id = path,
//                        Type = InfoType.folder,
//                        Attributes = new Attributes
//                        {
//                            Name = directoryName,
//                            Readable = true,
//                            Writable = true,
//                            // created date, size vb.
//                            Created = /*GetUnixTimestamp*/(DateTime.Now),
//                            Modified = /*GetUnixTimestamp*/(DateTime.Now),
//                            Path = getPath(() => path)
//                        }
//                    }
//                };

//                return result;
//            }
//            else
//            {
//                var fileName = Path.GetFileName(filePath/*Path.Combine(_webRootPath, path)*/);
//                var fileExtension = Path.GetExtension(fileName).Replace(".", "");
//                //var filePath = Path.Combine(_webRootPath, path);
//                _fileStore.TryDeleteFileAsync(filePath).Wait();
//                …æ≥˝Àı¬‘Õº(filePath);

//                var result = new Result<ResultInfo>
//                {
//                    Data = new ResultInfo
//                    {
//                        Id = path,
//                        Type = InfoType.file,
//                        Attributes = new Attributes
//                        {
//                            Name = fileName,
//                            Extension = fileExtension,
//                            Readable = true,
//                            Writable = true,
//                            Created = /*GetUnixTimestamp*/(DateTime.Now),
//                            Modified = /*GetUnixTimestamp*/(DateTime.Now)
//                            // Path = $"/{fileName}"
//                        }
//                    }
//                };

//                return result;
//            }
//        }
//        public byte[] ReadFile(string path)
//        {
//            var filePath = Path.Combine(_webRootPath, path);

//            using (var stream = _fileStore.GetFileStreamAsync(filePath).Result)
//            {
//                var fileBytes = new byte[stream.Length];
//                stream.Read(fileBytes, 0, fileBytes.Length);
//                return fileBytes;
//            }

//            //byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
//        }
//        public byte[] GetImage(string path, bool thumbnail)
//        {
//            if (thumbnail)
//            {
//                var filePath = Path.Combine(_webRootPath, path);

//                var thumbnailPath = getThumbnailPath(filePath);
//                if (_fileStore.FileExists(thumbnailPath))
//                {
//                    return ReadFile(thumbnailPath);
//                }
//                else
//                {
//                    using (var image = Image.Load(_fileStore.GetFileStreamAsync(filePath).Result))
//                    {
//                        image.Mutate(x => x.Resize(180, 100));
//                        using (MemoryStream stream = new MemoryStream())
//                        {
//                            image.SaveAsPng(stream);
//                            var buffer = stream.GetBuffer();
//                            //File.WriteAllBytes(thumbnailPath, buffer);
//                            stream.Position = 0;
//                            _fileStore.CreateFileFromStreamAsync(thumbnailPath, stream);
//                            return buffer;
//                        }
//                    }
//                }
//            }
//            else
//            {
//                return ReadFile(path);
//            }
//        }
//        public Result<ResultInfo> Summarize()
//        {
//            //// Get Dir count
//            //var directories = Directory.GetDirectories(_webRootPath, "*", SearchOption.AllDirectories).Length;

//            //// Get file count
//            //var directoryInfo = new DirectoryInfo(_webRootPath);
//            //var files = directoryInfo.GetFiles("*", SearchOption.AllDirectories);

//            //// Get combined file sizes
//            //var allSize = files.Select(f => f.Length).Sum();

//            //var result = new Result<ResultInfo>
//            //{
//            //    Data = new ResultInfo
//            //    {
//            //        Id = "/",
//            //        Type = InfoType.summary,
//            //        Attributes = new Attributes
//            //        {
//            //            Size = allSize,
//            //            Files = files.Length,
//            //            Folders = directories,
//            //            SizeLimit = 0
//            //        }
//            //    }
//            //};

//            //return result;
//            return null;
//        }

//        //private static void DirectoryCopy(string sourceDirName, string destDirName)
//        //{
//        //    // Get the subdirectories for the specified directory.
//        //    var dir = new DirectoryInfo(sourceDirName);

//        //    if (!dir.Exists)
//        //    {
//        //        throw new DirectoryNotFoundException(
//        //            "Source directory does not exist or could not be found: "
//        //            + sourceDirName);
//        //    }

//        //    var dirs = dir.GetDirectories();
//        //    // If the destination directory doesn't exist, create it.
//        //    if (!Directory.Exists(destDirName))
//        //    {
//        //        Directory.CreateDirectory(destDirName);
//        //    }

//        //    // Get the files in the directory and copy them to the new location.
//        //    var files = dir.GetFiles();
//        //    foreach (var file in files)
//        //    {
//        //        var temppath = Path.Combine(destDirName, file.Name);
//        //        file.CopyTo(temppath, false);
//        //    }

//        //    // If copying subdirectories, copy them and their contents to new location.
//        //    foreach (var subdir in dirs)
//        //    {
//        //        var temppath = Path.Combine(destDirName, subdir.Name);
//        //        DirectoryCopy(subdir.FullName, temppath);
//        //    }
//        //}

//        private static string MakeWebPath(string path, bool addSeperatorToBegin = false, bool addSeperatorToLast = false)
//        {
//            path = path.Replace("\\", "/");

//            if (addSeperatorToBegin) path = "/" + path;
//            if (addSeperatorToLast) path = path + "/";

//            return path;
//        }
//        //private Int32 GetUnixTimestamp(DateTime dt)
//        //{
//        //    return (Int32)(dt.ToUniversalTime().Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
//        //}
//        private static string GetRelativePath(string baseDirPath, string subFullPath)
//        {
//            // ForceBasePath to a path 
//            if (!baseDirPath.EndsWith("\\"))
//                baseDirPath += "\\";
//            var baseUri = new Uri(baseDirPath);
//            var fullUri = new Uri(subFullPath);
//            var relativeUri = baseUri.MakeRelativeUri(fullUri);
//            // Uri's use forward slashes so convert back to backward slashes 
//            return relativeUri.ToString().Replace("/", "\\");
//        }
//        private string getPath(Func<string> func)
//        {
//            if (_publicPath)
//                return func();
//            else
//                return null;
//        }
//        //private bool IsImage(FileInfo fileInfo)
//        //{
//        //    foreach (string ext in imgExtensions)
//        //    {
//        //        if (Path.GetExtension(fileInfo.FullName).ToLower() == ext.ToLower())
//        //        {
//        //            return true;
//        //        }
//        //    }
//        //    return false;
//        //}

//        //private string getMimeMapping(string fileName)
//        //{
//        //    var fileExt = System.IO.Path.GetExtension(fileName);
//        //    if (fileExtensionContentTypeProvider.Mappings.ContainsKey(fileExt))
//        //        return fileExtensionContentTypeProvider.Mappings[fileExt];
//        //    else
//        //        return "application/x-msdownload";
//        //}
//        private string thumbnailExt = "_thumbnail_";
//        private bool  «∑ÒÀı¬‘Õº(IFileStoreEntry fileStoreEntry)
//        {
//            if (fileStoreEntry.IsDirectory)
//                return false;
//            return  «∑ÒÀı¬‘Õº(fileStoreEntry.Name);
//        }
//        private bool  «∑ÒÀı¬‘Õº(string fileName)
//        {
//            var _fileName = System.IO.Path.GetFileName(fileName);
//            return _fileName.StartsWith(thumbnailExt);
//        }
//        private string getThumbnailPath(string path)
//        {
//            var thumbnailPath = Path.Combine(Path.GetDirectoryName(path), thumbnailExt + Path.GetFileName(path));
//            return thumbnailPath;
//        }
//        private void …æ≥˝Àı¬‘Õº(string fileName)
//        {
//            var thumbnailPath = getThumbnailPath(fileName);
//            if (_fileStore.FileExists(thumbnailPath))
//                _fileStore.TryDeleteFileAsync(thumbnailPath).Wait();
//        }
//    }
//}
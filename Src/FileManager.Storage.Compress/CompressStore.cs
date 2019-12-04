using FileManager.FileStorage;
using SharpCompress.Readers;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FileManager.Storage.Compress
{
    public class CompressStore : IFileStore
    {
        private readonly System.IO.Stream _zipFileStream;
        private readonly string _password;
        public CompressStore(System.IO.Stream zipFileStream) : this(zipFileStream, null)
        {
        }
        public CompressStore(System.IO.Stream zipFileStream, string password)
        {
            _zipFileStream = zipFileStream;
            _password = password;
        }
        public async Task<IFileStoreEntry> GetFileInfoAsync(string path)
        {
            return await Task.Run<IFileStoreEntry>(() =>
            {
                _zipFileStream.Position = 0;
                using (var reader = ReaderFactory.Open(_zipFileStream, new ReaderOptions { Password = _password }))
                {
                    while (reader.MoveToNextEntry())
                    {
                        if (!reader.Entry.IsDirectory && 路径转换(reader.Entry.Key, true).Equals(路径转换(path, false), StringComparison.OrdinalIgnoreCase))
                        {
                            return new CompressStoreEntry(path, reader.Entry);
                        }
                    }
                }
                return null;
            });
        }
        public async Task<IFileStoreEntry> GetDirectoryInfoAsync(string path)
        {
            return await Task.Run<IFileStoreEntry>(() =>
            {
                _zipFileStream.Position = 0;
                using (var reader = ReaderFactory.Open(_zipFileStream, new ReaderOptions { Password = _password }))
                {
                    while (reader.MoveToNextEntry())
                    {
                        if (reader.Entry.IsDirectory && 是否当前目录(路径转换(path, false), 路径转换(reader.Entry.Key, true)))
                        {
                            return new CompressStoreEntry(path, reader.Entry);
                        }
                    }
                }
                return null;
            });
        }
        public async Task<IEnumerable<IFileStoreEntry>> GetDirectoryContentAsync(string path = null, bool includeSubDirectories = false)
        {
            return await Task.Run<IEnumerable<IFileStoreEntry>>(() =>
            {
                try
                {
                    _zipFileStream.Position = 0;
                    using (var reader = ReaderFactory.Open(_zipFileStream, new ReaderOptions { Password = _password }))
                    {
                        var list = new List<IFileStoreEntry>();
                        while (reader.MoveToNextEntry())
                        {
                            if (是否当前目录(路径转换(path, false), 路径转换(reader.Entry.Key, true)))
                            {
                                list.Add(new CompressStoreEntry(path, reader.Entry));
                            }
                        }
                        return list;
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            });
        }
        public  Task<bool> TryCreateDirectoryAsync(string path)
        {
            throw new FileStoreException($"不支持.");
        }
        public  Task<bool> TryDeleteFileAsync(string path)
        {
            throw new FileStoreException($"不支持.");
        }
        public  Task<bool> TryDeleteDirectoryAsync(string path)
        {
            throw new FileStoreException($"不支持.");
        }
        public  Task MoveFileAsync(string oldPath, string newPath)
        {
            throw new FileStoreException($"不支持.");
        }

        public  Task CopyFileAsync(string srcPath, string dstPath)
        {
            throw new FileStoreException($"不支持.");
        }

        public async Task<Stream> GetFileStreamAsync(string path)
        {
            return await Task.Run<Stream>(() =>
            {
                _zipFileStream.Position = 0;
                using (var reader = ReaderFactory.Open(_zipFileStream, new ReaderOptions { Password = _password }))
                {
                    var list = new List<IFileStoreEntry>();
                    while (reader.MoveToNextEntry())
                    {
                        if (!reader.Entry.IsDirectory && 路径转换(reader.Entry.Key, true).Equals(路径转换(path, false), StringComparison.OrdinalIgnoreCase))
                        {
                            return reader.OpenEntryStream();
                        }
                    }
                    return null;
                }
            });
        }
        public async Task<Stream> GetFileStreamAsync(IFileStoreEntry fileStoreEntry)
        {
            return await GetFileStreamAsync(fileStoreEntry.Path);
        }
        public  Task CreateFileFromStreamAsync(string path, Stream inputStream, bool overwrite = false)
        {
            throw new FileStoreException($"不支持.");
        }

        //public Task<bool> IsDirectory(string path)
        //{
        //    //var result = _ftpClient.FileExists(path);
        //    //return Task.FromResult(!result);
        //    var result = _ftpClient.DirectoryExists(path);
        //    return Task.FromResult(result);

        //}

        //public bool DirectoryExists(string newPath)
        //{
        //    return _ftpClient.DirectoryExists(newPath);
        //}

        public  Task MoveDirectoryAsync(string oldPath, string newPath)
        {
            throw new FileStoreException($"不支持.");
        }

        //public bool FileExists(string newPath)
        //{
        //    return _ftpClient.FileExists(newPath);
        //}

        //public void CopyDirectoryAsync(string oldPath, string newPath)
        //{
        //    DirectoryCopy(oldPath, newPath);
        //}
        //private async Task Copy(string sourceFileName, string destFileName)
        //{
        //    using (var readStream = await _ftpClient.OpenReadAsync(sourceFileName))
        //    {
        //        await _ftpClient.UploadAsync(readStream, destFileName);
        //        //using (var writeStream = _ftpClient.OpenWrite(destFileName))
        //        //{

        //        //    readStream.CopyToAsync(writeStream).Wait();
        //        //}
        //    }
        //}



        //private void DirectoryCopy(string sourceDirName, string destDirName)
        //{
        //    //// Get the subdirectories for the specified directory.
        //    //var dir = _ftpClient.GetListingAsync(sourceDirName, FluentFTP.FtpListOption.ForceList);

        //    //if (!dir.Exists)
        //    //{
        //    //    throw new DirectoryNotFoundException(
        //    //        "Source directory does not exist or could not be found: "
        //    //        + sourceDirName);
        //    //}

        //    //var dirs = dir.GetDirectories();
        //    //// If the destination directory doesn't exist, create it.
        //    //if (!Directory.Exists(destDirName))
        //    //{
        //    //    Directory.CreateDirectory(destDirName);
        //    //}

        //    //// Get the files in the directory and copy them to the new location.
        //    //var files = dir.GetFiles();
        //    //foreach (var file in files)
        //    //{
        //    //    var temppath = Path.Combine(destDirName, file.Name);
        //    //    file.CopyTo(temppath, false);
        //    //}

        //    //// If copying subdirectories, copy them and their contents to new location.
        //    //foreach (var subdir in dirs)
        //    //{
        //    //    var temppath = Path.Combine(destDirName, subdir.Name);
        //    //    DirectoryCopy(subdir.FullName, temppath);
        //    //}
        //}

        private string 路径转换(string path, bool iszip)
        {
            var up = path.Replace("/", "\\");
            if (iszip)
            {
                up = "\\" + string.Join("\\", up.Split(new string[] { "\\" }, StringSplitOptions.RemoveEmptyEntries).Skip(1));
            }

            return up;
        }
        private bool 是否当前目录(string path, string subpath)
        {
            var l = path.Split(new string[] { "\\" }, StringSplitOptions.RemoveEmptyEntries).Length + 1;
            return
                subpath.StartsWith(path, StringComparison.OrdinalIgnoreCase) &&
                subpath.Split(new string[] { "\\" }, StringSplitOptions.RemoveEmptyEntries).Length == l;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FileManager.FileStorage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FileManager.Controllers
{
    [Route("api/FileStorage/[controller]/[action]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private const int MaxListFiles = 100;

        private readonly IFileStore _fileStore;
        private readonly FileStorePermissionGuard _guard;

        public FileController(IFileStore fileStore, DefaultPermissionHandle permissions)
        {
            _fileStore = fileStore;
            _guard = new FileStorePermissionGuard(fileStore, permissions ?? throw new ArgumentNullException(nameof(permissions)));
        }

        [HttpPost]
        public async Task<ActionResult<bool>> UploadAsync([FromBody] UploadFileInput model, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model?.Path))
                    return BadRequest("Path is required.");
                if (model.FileBytes == null || model.FileBytes.Length == 0)
                    return BadRequest("FileBytes is required.");

                model.Path = VirtualPath.Normalize(model.Path);
                var parent = GetParentPath(model.Path);
                await _guard.EnsureUploadAsync(parent, cancellationToken);
                using (var stream = new MemoryStream(model.FileBytes))
                {
                    await _fileStore.CreateFileFromStreamAsync(model.Path, stream, cancellationToken: cancellationToken);
                }
                return true;
            }
            catch (FileStoreException ex) when (ex.Message == "Access denied.")
            {
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }
            catch (FileStoreException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> DownloadAsync(string path, CancellationToken cancellationToken)
        {
            try
            {
                path = VirtualPath.Normalize(path);
                await _guard.EnsureDownloadAsync(path, cancellationToken);
                var stream = await _fileStore.GetFileStreamAsync(path, cancellationToken);
                var fileName = Path.GetFileName(path);
                return File(stream, "application/octet-stream", fileName);
            }
            catch (FileStoreException ex) when (ex.Message == "Access denied.")
            {
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }
            catch (FileStoreException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult<byte[]>> GetFileAsync(string path, CancellationToken cancellationToken)
        {
            try
            {
                path = VirtualPath.Normalize(path);
                await _guard.EnsureDownloadAsync(path, cancellationToken);
                using (var stream = await _fileStore.GetFileStreamAsync(path, cancellationToken))
                using (var memoryStream = new MemoryStream())
                {
                    await stream.CopyToAsync(memoryStream);
                    return memoryStream.ToArray();
                }
            }
            catch (FileStoreException ex) when (ex.Message == "Access denied.")
            {
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }
            catch (FileStoreException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult<List<FileListItem>>> GetListFilesAsync(string path, string extension, CancellationToken cancellationToken)
        {
            try
            {
                path = VirtualPath.Normalize(path);
                await _guard.EnsureListAsync(path, cancellationToken);
                var directoryInfo = await _fileStore.GetDirectoryContentAsync(path, true, cancellationToken);
                var items = new List<FileListItem>();
                foreach (var file in directoryInfo.Where(f => !f.IsDirectory && string.Equals(Path.GetExtension(f.Path), extension, StringComparison.OrdinalIgnoreCase)).Take(MaxListFiles))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    items.Add(new FileListItem
                    {
                        Path = file.Path,
                        Name = file.Name,
                        Length = file.Length
                    });
                }

                return items;
            }
            catch (FileStoreException ex) when (ex.Message == "Access denied.")
            {
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }
            catch (FileStoreException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        public async Task<ActionResult<bool>> DeleteFileAsync(string path, CancellationToken cancellationToken)
        {
            try
            {
                path = VirtualPath.Normalize(path);
                await _guard.EnsureDeleteAsync(path, cancellationToken);
                return await _fileStore.TryDeleteFileAsync(path, cancellationToken);
            }
            catch (FileStoreException ex) when (ex.Message == "Access denied.")
            {
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }
            catch (FileStoreException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        public async Task<ActionResult<bool>> DeleteDirectoryAsync(string path, CancellationToken cancellationToken)
        {
            try
            {
                path = VirtualPath.Normalize(path);
                await _guard.EnsureDeleteAsync(path, cancellationToken);
                return await _fileStore.TryDeleteDirectoryAsync(path, cancellationToken);
            }
            catch (FileStoreException ex) when (ex.Message == "Access denied.")
            {
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }
            catch (FileStoreException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private static string GetParentPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;
            var slash = path.LastIndexOf('/');
            return slash < 0 ? string.Empty : path.Substring(0, slash);
        }

        public class UploadFileInput
        {
            public byte[] FileBytes { get; set; }
            public string Path { get; set; }
        }
    }
}

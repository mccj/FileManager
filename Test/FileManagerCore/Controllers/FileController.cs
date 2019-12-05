using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FileManagerCore.Controllers
{
    //[Route("api/[controller]")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly FileManager.FileStorage.IFileStore _fileStore;
        public FileController(FileManager.FileStorage.IFileStore fileStore)
        {
            _fileStore = fileStore;
        }
        [HttpPost]
        public async Task UploadAsync(UploadFileInput model)
        {
            if (string.IsNullOrWhiteSpace(model.Path))
                throw new ArgumentNullException(nameof(model.Path));
            if (model.FileBytes == null || model.FileBytes.Length == 0)
                throw new ArgumentNullException(nameof(model.FileBytes));
            var stream = new System.IO.MemoryStream(model.FileBytes);

            await _fileStore.CreateFileFromStreamAsync(model.Path, stream);
        }
        [HttpPost]
        public async Task<byte[]> GetFileAsync(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException(nameof(path));

            using (var r = await _fileStore.GetFileStreamAsync(path))
            {
                var stream = new System.IO.MemoryStream();
                await r.CopyToAsync(stream);
                return stream.GetBuffer();
            }
        }
        [HttpDelete]
        public async Task<bool> DeleteFileAsync(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException(nameof(path));

            return await _fileStore.TryDeleteFileAsync(path);
        }
        public class UploadFileInput
        {
            public byte[] FileBytes { get; set; }
            public string Path { get; set; }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FileManagerCore.Controllers
{
    //[Route("api/[controller]")]
    [Route("api/FileStorage/[controller]/[action]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly FileManager.FileStorage.IFileStore _fileStore;
        public FileController(FileManager.FileStorage.IFileStore fileStore)
        {
            _fileStore = fileStore;
        }
        [HttpPost]
        public async Task<ActionResult<bool>> UploadAsync(UploadFileInput model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.Path))
                    throw new ArgumentNullException(nameof(model.Path));
                if (model.FileBytes == null || model.FileBytes.Length == 0)
                    throw new ArgumentNullException(nameof(model.FileBytes));
                var stream = new System.IO.MemoryStream(model.FileBytes);

                await _fileStore.CreateFileFromStreamAsync(model.Path, stream);
                return true;
            }
            catch (Exception ex)
            {
                return BadRequest(ModelState);
                //throw ex;
            }
        }
        [HttpPost]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<byte[]>> GetFileAsync(string path)
        {
            try
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
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        [HttpDelete]
        public async Task<ActionResult<bool>> DeleteFileAsync(string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path))
                    throw new ArgumentNullException(nameof(path));

                return await _fileStore.TryDeleteFileAsync(path);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        [HttpDelete]
        public async Task<ActionResult<bool>> DeleteDirectoryAsync(string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path))
                    throw new ArgumentNullException(nameof(path));

                return await _fileStore.TryDeleteDirectoryAsync(path);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        public class UploadFileInput
        {
            public byte[] FileBytes { get; set; }
            public string Path { get; set; }
        }
    }
}
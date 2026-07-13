using FileManager.FileStorage;
using FileManager.FileStorage.Standard;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FileManager.Storage.Standard.Test
{
    public class FileSystemStoreTests : IDisposable
    {
        private readonly string _root;
        private readonly FileSystemStore _store;

        public FileSystemStoreTests()
        {
            _root = Path.Combine(Path.GetTempPath(), "FileManager.Sample.Minimals", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_root);
            _store = new FileSystemStore(_root);
        }

        [Fact]
        public async Task CreateReadDeleteFile_Works()
        {
            await using (var input = new MemoryStream("hello"u8.ToArray()))
            {
                await _store.CreateFileFromStreamAsync("folder/sample.txt", input);
            }

            var entry = await _store.GetFileInfoAsync("folder/sample.txt");
            Assert.NotNull(entry);
            Assert.Equal("sample.txt", entry.Name);
            Assert.False(entry.IsDirectory);

            await using var stream = await _store.GetFileStreamAsync("folder/sample.txt");
            using var reader = new StreamReader(stream);
            Assert.Equal("hello", await reader.ReadToEndAsync());
            await stream.DisposeAsync();

            Assert.True(await _store.TryDeleteFileAsync("folder/sample.txt"));
        }

        [Fact]
        public async Task CopyDirectory_Works()
        {
            await _store.TryCreateDirectoryAsync("src/nested");
            await using (var input = new MemoryStream("copy-me"u8.ToArray()))
            {
                await _store.CreateFileFromStreamAsync("src/nested/file.txt", input);
            }

            await _store.CopyDirectoryAsync("src/nested", "dst");

            var copied = await _store.GetFileInfoAsync("dst/file.txt");
            Assert.NotNull(copied);
        }

        [Fact]
        public async Task RejectsPathTraversal()
        {
            await Assert.ThrowsAsync<FileStoreException>(() => _store.GetFileInfoAsync("../outside.txt"));
        }

        [Fact]
        public async Task MoveDirectory_RejectsExistingTargetFile()
        {
            await _store.TryCreateDirectoryAsync("move-src");
            await _store.TryCreateDirectoryAsync("move-dst");
            await using (var input = new MemoryStream("x"u8.ToArray()))
            {
                await _store.CreateFileFromStreamAsync("target-file.txt", input);
            }

            await Assert.ThrowsAsync<FileStoreException>(() => _store.MoveDirectoryAsync("move-src", "target-file.txt"));
        }

        public void Dispose()
        {
            if (Directory.Exists(_root))
                Directory.Delete(_root, recursive: true);
        }
    }

    public class FileProviderStoreTests
    {
        [Fact]
        public async Task ReadOnlyStore_RejectsWrite()
        {
            var provider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(Path.GetTempPath());
            var store = new FileProviderStore(provider);

            await Assert.ThrowsAsync<FileManager.FileStorage.FileStoreException>(
                () => store.CreateFileFromStreamAsync("demo.txt", new MemoryStream()));
        }

        [Fact]
        public async Task GetDirectoryContent_ReturnsEntries()
        {
            var root = Path.Combine(Path.GetTempPath(), "FileManagerProviderTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            await File.WriteAllTextAsync(Path.Combine(root, "readme.txt"), "ok");

            try
            {
                var provider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(root);
                var store = new FileProviderStore(provider);
                var entries = await store.GetDirectoryContentAsync(null, includeSubDirectories: false);
                Assert.Contains(entries, e => e.Name == "readme.txt" && !e.IsDirectory);
            }
            finally
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }
}

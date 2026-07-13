using FileManager.FileStorage;
using FileManager.Storage.Composite;
using FileManager.Storage.InMemory;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FileManager.Storage.Composite.Test
{
    public class CompositeFileStoreTests
    {
        [Fact]
        public async Task ListsDrivesAtRoot()
        {
            var store = new CompositeFileStoreBuilder()
                .AddDrive("a", new InMemoryStore())
                .AddDrive("b", new InMemoryStore())
                .Build(serviceProvider: null);

            var drives = (await store.GetDirectoryContentAsync()).ToArray();
            Assert.Equal(2, drives.Length);
            Assert.Contains(drives, d => d.Name == "a" && d.IsDirectory);
            Assert.Contains(drives, d => d.Name == "b" && d.IsDirectory);
        }

        [Fact]
        public async Task RoutesToNamedDrive()
        {
            var memory = new InMemoryStore();
            await using (var input = new MemoryStream("drive-a"u8.ToArray()))
            {
                await memory.CreateFileFromStreamAsync("note.txt", input);
            }

            var store = new CompositeFileStoreBuilder()
                .AddDrive("local", memory)
                .Build(serviceProvider: null);

            var entry = await store.GetFileInfoAsync("local/note.txt");
            Assert.NotNull(entry);
            Assert.Equal("note.txt", entry.Name);
            Assert.Equal("local/note.txt", entry.Path);
        }

        [Fact]
        public void DrivePath_ParsesExplorerSyntax()
        {
            Assert.True(DrivePath.TryParseExplorerPath("[local]:docs/file.txt", out var path));
            Assert.Equal("local/docs/file.txt", path);
        }
    }
}

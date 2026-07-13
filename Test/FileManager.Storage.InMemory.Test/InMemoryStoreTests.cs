using FileManager.FileStorage;
using FileManager.Storage.InMemory;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FileManager.Storage.InMemory.Test
{
    public class InMemoryStoreTests
    {
        [Fact]
        public async Task CreateReadDeleteFile_Works()
        {
            var store = new InMemoryStore();
            await using (var input = new MemoryStream("hello"u8.ToArray()))
            {
                await store.CreateFileFromStreamAsync("folder/sample.txt", input);
            }

            var entry = await store.GetFileInfoAsync("folder/sample.txt");
            Assert.NotNull(entry);
            Assert.Equal("sample.txt", entry.Name);

            await using var stream = await store.GetFileStreamAsync("folder/sample.txt");
            using var reader = new StreamReader(stream);
            Assert.Equal("hello", await reader.ReadToEndAsync());

            Assert.True(await store.TryDeleteFileAsync("folder/sample.txt"));
        }

        [Fact]
        public async Task ListsRootAndNestedDirectories()
        {
            var store = new InMemoryStore();
            await store.TryCreateDirectoryAsync("docs");
            await using (var input = new MemoryStream("x"u8.ToArray()))
            {
                await store.CreateFileFromStreamAsync("docs/readme.txt", input);
            }

            var root = (await store.GetDirectoryContentAsync()).ToArray();
            Assert.Contains(root, e => e.IsDirectory && e.Name == "docs");

            var docs = (await store.GetDirectoryContentAsync("docs")).ToArray();
            Assert.Single(docs);
            Assert.Equal("readme.txt", docs[0].Name);
        }
    }
}

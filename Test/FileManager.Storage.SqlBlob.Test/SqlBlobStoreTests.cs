using FileManager.Storage.SqlBlob;
using Microsoft.Data.Sqlite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FileManager.Storage.SqlBlob.Test
{
    public class SqlBlobStoreTests
    {
        [Fact]
        public async Task CreateReadDeleteFile_Works()
        {
            var dbPath = Path.Combine(Path.GetTempPath(), "fm-sqlblob-" + Path.GetRandomFileName() + ".db");
            var store = new SqlBlobStore("Data Source=" + dbPath + ";Pooling=false");
            try
            {
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
            finally
            {
                SqliteConnection.ClearAllPools();
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task ListsRootAndNestedDirectories()
        {
            var dbPath = Path.Combine(Path.GetTempPath(), "fm-sqlblob-" + Path.GetRandomFileName() + ".db");
            var store = new SqlBlobStore("Data Source=" + dbPath + ";Pooling=false");
            try
            {
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
            finally
            {
                SqliteConnection.ClearAllPools();
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }
    }
}

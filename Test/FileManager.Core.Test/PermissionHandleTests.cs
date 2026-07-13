using FileManager.FileStorage;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FileManager.Core.Test
{
    public class PermissionHandleTests
    {
        [Fact]
        public async Task DefaultPermissionHandle_WithoutHandlers_ReturnsReadOnly()
        {
            var handle = new DefaultPermissionHandle(Enumerable.Empty<IPermissions>());
            var permission = await handle.GetPermissionAsync(new TestEntry());
            Assert.Equal(PermissionDefaults.ReadOnly, permission);
        }

        [Fact]
        public async Task DefaultPermissionHandle_MergesMultipleHandlers()
        {
            var handle = new DefaultPermissionHandle(new IPermissions[]
            {
                new DefaultPermissions(_ => Task.FromResult(Permission.Download)),
                new DefaultPermissions(_ => Task.FromResult(Permission.Upload))
            });

            var permission = await handle.GetPermissionAsync(new TestEntry());
            Assert.Equal(Permission.Download | Permission.Upload, permission);
        }

        private sealed class TestEntry : IFileStoreEntry
        {
            public string Path => "sample.txt";
            public string Name => "sample.txt";
            public string DirectoryPath => string.Empty;
            public long Length => 1;
            public bool IsDirectory => false;
            public System.DateTimeOffset LastModifiedUtc => System.DateTimeOffset.UtcNow;
        }
    }
}

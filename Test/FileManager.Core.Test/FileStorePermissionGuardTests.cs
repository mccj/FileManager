using FileManager.FileStorage;
using FileManager.Storage.InMemory;
using System.Threading.Tasks;
using Xunit;

namespace FileManager.Core.Test
{
    public class FileStorePermissionGuardTests
    {
        [Fact]
        public async Task Upload_Denied_When_ReadOnly_Permissions()
        {
            var store = new InMemoryStore();
            var handle = new DefaultPermissionHandle(System.Linq.Enumerable.Empty<IPermissions>());
            var guard = new FileStorePermissionGuard(store, handle);

            await Assert.ThrowsAsync<FileStoreException>(() => guard.EnsureUploadAsync(string.Empty));
        }

        [Fact]
        public async Task CanWriteDirectory_False_When_ReadOnly_Permissions()
        {
            var store = new InMemoryStore();
            var handle = new DefaultPermissionHandle(System.Linq.Enumerable.Empty<IPermissions>());
            var guard = new FileStorePermissionGuard(store, handle);

            var canWrite = await guard.CanWriteDirectoryAsync(string.Empty);
            Assert.False(canWrite);
        }

        [Fact]
        public async Task Upload_Allowed_With_Full_Access_Handle()
        {
            var store = new InMemoryStore();
            var handle = new DefaultPermissionHandle(new IPermissions[]
            {
                new DefaultPermissions(_ => Task.FromResult(
                    Permission.ListSubFolders | Permission.ListFiles | Permission.Download | Permission.Preview
                    | Permission.Create | Permission.Upload | Permission.Delete | Permission.Rename | Permission.Copy))
            });
            var guard = new FileStorePermissionGuard(store, handle);

            await guard.EnsureUploadAsync(string.Empty);
            await store.CreateFileFromStreamAsync("a.txt", new System.IO.MemoryStream(new byte[] { 1 }));
            await guard.EnsureCopyAsync("a.txt", string.Empty);
        }

        [Fact]
        public async Task Copy_Denied_When_ReadOnly_Permissions()
        {
            var store = new InMemoryStore();
            var handle = new DefaultPermissionHandle(System.Linq.Enumerable.Empty<IPermissions>());
            var guard = new FileStorePermissionGuard(store, handle);

            await Assert.ThrowsAsync<FileStoreException>(() => guard.EnsureCopyAsync("a.txt", string.Empty));
        }
    }
}

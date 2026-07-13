using Xunit;

namespace FileManager.Core.Test
{
    public class StoreFileProviderTests
    {
        [Fact]
        public void CompositeFileProvider_ReturnsCombinedEntries()
        {
            var root = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "FileManagerComposite", System.Guid.NewGuid().ToString("N"));
            System.IO.Directory.CreateDirectory(root);
            System.IO.File.WriteAllText(System.IO.Path.Combine(root, "a.txt"), "a");

            try
            {
                var physical = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(root);
                var store = new FileManager.FileStorage.StoreFileProvider(new FileManager.FileStorage.Standard.FileSystemStore(root));
                var composite = new Microsoft.Extensions.FileProviders.CompositeFileProvider(physical, store);
                var contents = composite.GetDirectoryContents(string.Empty);
                Assert.True(contents.Exists);
                Assert.Contains(contents, item => item.Name == "a.txt");
            }
            finally
            {
                System.IO.Directory.Delete(root, recursive: true);
            }
        }
    }
}

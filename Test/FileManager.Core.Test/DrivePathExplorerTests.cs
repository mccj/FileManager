using FileManager.FileStorage;
using FileManager.Storage.Composite;
using FileManager.Storage.InMemory;
using Xunit;

namespace FileManager.Core.Test;

public class DrivePathExplorerTests
{
    [Fact]
    public void TryParseExplorerPath_SingleDriveRoot_MapsToEmptyStorePath()
    {
        var store = new InMemoryStore();

        Assert.True(DrivePath.TryParseExplorerPath("[Root]:", store, out var path));
        Assert.Equal(string.Empty, path);
    }

    [Fact]
    public void TryParseExplorerPath_SingleDriveNestedPath_StripsRootFolderName()
    {
        var store = new InMemoryStore();

        Assert.True(DrivePath.TryParseExplorerPath("[Root]:documents/file.txt", store, out var path));
        Assert.Equal("documents/file.txt", path);
    }

    [Fact]
    public void TryParseExplorerPath_CompositeDrive_PreservesDrivePrefix()
    {
        var store = new CompositeFileStoreBuilder()
            .AddDrive("local", new InMemoryStore())
            .Build(serviceProvider: null);

        Assert.True(DrivePath.TryParseExplorerPath("[local]:documents/file.txt", store, out var path));
        Assert.Equal("local/documents/file.txt", path);
    }

    [Fact]
    public void TryParseExplorerPath_CompositeDriveRoot_PreservesDriveName()
    {
        var store = new CompositeFileStoreBuilder()
            .AddDrive("local", new InMemoryStore())
            .Build(serviceProvider: null);

        Assert.True(DrivePath.TryParseExplorerPath("[local]:", store, out var path));
        Assert.Equal("local", path);
    }
}

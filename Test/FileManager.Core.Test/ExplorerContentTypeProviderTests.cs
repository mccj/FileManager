using Xunit;

namespace FileManager.Core.Test;

public class ExplorerContentTypeProviderTests
{
    [Theory]
    [InlineData("Program.cs", "text/plain; charset=utf-8")]
    [InlineData("appsettings.json", "application/json; charset=utf-8")]
    [InlineData("readme.txt", "text/plain; charset=utf-8")]
    [InlineData("unknown.bin", "application/octet-stream")]
    public void GetMimeMapping_ReturnsBrowserFriendlyTypes(string fileName, string expectedMime)
    {
        var mime = FileManager.ExplorerContentTypeProvider.GetMimeMapping(fileName);
        Assert.Equal(expectedMime, mime);
    }
}

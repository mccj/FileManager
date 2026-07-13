using Xunit;

namespace FileManager.Core.Test;

public class ExplorerHighlightLanguagesTests
{
    [Theory]
    [InlineData("Program.cs", "csharp")]
    [InlineData("App.xaml.cs", "csharp")]
    [InlineData("appsettings.json", "json")]
    [InlineData("FileManager.csproj", "xml")]
    [InlineData("readme.txt", "plaintext")]
    [InlineData("script.sh", "bash")]
    public void GetHighlightLanguage_MapsKnownExtensions(string fileName, string expectedLanguage)
    {
        var language = FileManager.ExplorerHighlightLanguages.GetHighlightLanguage(fileName);
        Assert.Equal(expectedLanguage, language);
    }
}

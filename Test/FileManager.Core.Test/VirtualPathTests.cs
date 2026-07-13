using FileManager.FileStorage;
using System;
using Xunit;

namespace FileManager.Core.Test
{
    public class VirtualPathTests
    {
        [Fact]
        public void Normalize_RemovesLeadingSlash()
        {
            Assert.Equal("folder/file.txt", VirtualPath.Normalize("/folder/file.txt"));
        }

        [Fact]
        public void Normalize_RejectsParentTraversal()
        {
            Assert.Throws<FileStoreException>(() => VirtualPath.Normalize("folder/../secret.txt"));
        }

        [Fact]
        public void ValidateFileName_RejectsPathSeparator()
        {
            Assert.Throws<FileStoreException>(() => VirtualPath.ValidateFileName("../secret.txt"));
        }

        [Fact]
        public void IsExtensionAllowed_MatchesConfiguredList()
        {
            Assert.True(VirtualPath.IsExtensionAllowed("photo.JPG", new[] { "jpg", "png" }));
            Assert.False(VirtualPath.IsExtensionAllowed("script.exe", new[] { "jpg", "png" }));
        }
    }
}

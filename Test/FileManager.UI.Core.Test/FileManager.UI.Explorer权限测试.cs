using FileManager.FileStorage;
using Microsoft.Extensions.FileProviders;
using System;
using Xunit;

namespace FileManager.UI.Core.Test
{
    public class »®œﬁ≤‚ ‘
    {
        [Fact]
        public void ≤‚ ‘()
        {
            var read = Permission.ListSubFolders | Permission.ListFiles | Permission.Download | Permission.Copy | Permission.Preview | Permission.Print;
            var s1 = read.ToString();

            var full = Permission.ListSubFolders | Permission.ListFiles | Permission.Download | Permission.Copy | Permission.Preview | Permission.Print
                | Permission.Create | Permission.Delete | Permission.Rename | Permission.Edit | Permission.Upload | Permission.Compress | Permission.Extract | Permission.Cut | Permission.Paste | Permission.CreatePublicLink;
            var s2 = full.ToString();
        }
    }
}

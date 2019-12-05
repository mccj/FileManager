using Microsoft.Extensions.FileProviders;
using System;
using Xunit;

namespace FileManager.UI.Core.Test
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            //Microsoft.Extensions.FileProviders.Composite.CompositeDirectoryContents
            //    Microsoft.Extensions.FileProviders.NotFoundDirectoryContents
            //Microsoft.Extensions.FileProviders.Internal.PhysicalDirectoryContents
            //Microsoft.Extensions.FileProviders.PhysicalFileProvider
            var sss1 = new EmbeddedFileProvider(typeof(UnitTest1).Assembly);
            //var sss2 = new ManifestEmbeddedFileProvider(typeof(UnitTest1).Assembly);
            var sss3 = new PhysicalFileProvider(@"D:\Users\mccj\Source\Repos\FileManager\FileManager\");
            var sss4 = new NullFileProvider();
            var sss5 = new FileStorage.StoreFileProvider(new FileStorage.Standard.PhysicalFileSystemStore(@"D:\Users\mccj\Source\Repos\FileManager\FileManager\"));
            var sss6 = new FileStorage.StoreFileProvider(new FileStorage.Standard.FileSystemStore(@"D:\Users\mccj\Source\Repos\FileManager\FileManager\"));
            var sss7 = new CompositeFileProvider(sss1, sss3, sss4, sss5, sss6);

            var sdfsd = sss4.Watch("");
            var rr = sss4.GetFileInfo("");

            var sss11 = sss3.GetFileInfo("aaaa");
            var sssd11 = sss5.GetFileInfo("aaaa");
            var sssc11 = sss6.GetFileInfo("aaaa");

            var sss113 = sss3.GetFileInfo("FileProviderStore");
            var sssd113 = sss5.GetFileInfo("FileProviderStore");
            var sssd213 = sss6.GetFileInfo("FileProviderStore");

            var sss112 = sss3.GetFileInfo("ApplicationBuilderExtensions.cs");
            var sssd112 = sss5.GetFileInfo("ApplicationBuilderExtensions.cs");
            var sssc112 = sss6.GetFileInfo("ApplicationBuilderExtensions.cs");


            var sss = sss3.GetDirectoryContents("");
            var sssd = sss5.GetDirectoryContents("");
            var sssc = sss6.GetDirectoryContents("");

            var sss75 = sss3.GetDirectoryContents("aaaaa");
            var sssd5 = sss5.GetDirectoryContents("aaaaa");
            var sssdc = sss6.GetDirectoryContents("aaaaa");

            var sss76 = sss3.GetDirectoryContents("FileProviderStore");
            var sssd6 = sss5.GetDirectoryContents("FileProviderStore");
            var sssdcc = sss6.GetDirectoryContents("FileProviderStore");


            var sdsdasd = sss7.GetDirectoryContents("");
            //sss6.GetDirectoryContents("");
            //sss6.GetFileInfo("");
        }

        [Fact]
        public void Test2()
        {
            var clientParams = new WebDav.WebDavClientParams
            {
                BaseAddress = new Uri("http://10.11.11.11/remote.php/webdav/"),
                Credentials = new System.Net.NetworkCredential("mccj", "`1q2w3e4r")
            };
            var _client = new WebDav.WebDavClient(clientParams);
            var result = _client.Propfind("").Result;
            var result1 = _client.Propfind("/").Result;
            var result3 = _client.Propfind("/ddddddd").Result;

            var result4 = _client.Propfind("Nextcloud.mp4").Result;
            var ddddd = new WebDav.ProppatchParameters();
            var result6 = _client.Proppatch("Nextcloud.mp4",ddddd).Result;

        }
    }
}

using NUnit.Framework;

namespace FileManagerSdkTest
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            var rr = new FileManagerSdk.FileClient() { BaseUrl = "http://localhost:9261/" };
            var bytes = System.IO.File.ReadAllBytes(@"d:\Users\mccj\Desktop\timg.jpg");
            rr.File_Upload(new FileManagerSdk.Models.UploadFileInput { FileBytes = bytes, Path = "/aa/bb/cc.jpg" });

            var ddff = rr.File_GetFile("/aa/bb/cc.jpg");
            System.IO.File.WriteAllBytes("d:\\aaaa.png", ddff);
            Assert.Pass();
        }
    }
}
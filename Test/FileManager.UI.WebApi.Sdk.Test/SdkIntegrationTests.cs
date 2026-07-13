using NUnit.Framework;

namespace FileManager.UI.WebApi.Sdk.Test
{
    [TestFixture]
    [Category("Integration")]
    public class SdkIntegrationTests
    {
        [Test]
        [Explicit("Requires a running FileManager host at the configured base URL.")]
        public void UploadFile_RequiresRunningHost()
        {
            Assert.Inconclusive("Start FileManager.Sample.Full sample and configure SDK base URL before running this integration test.");
        }
    }
}

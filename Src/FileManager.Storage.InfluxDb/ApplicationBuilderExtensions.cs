using FileManager.Storage.InfluxDb;
using System;

namespace FileManager
{
    public static class InfluxDbStoreApplicationBuilderExtensions
    {
        /// <summary>注册 InfluxDB 时序实验性存储。</summary>
        public static FileServicesOptions AddInfluxDbStore(this FileServicesOptions app, string url, string token, string org, string bucket)
        {
            return app.AddFileStore(_ => new InfluxDbStore(url, token, org, bucket));
        }

        /// <summary>注册 InfluxDB 时序实验性存储。</summary>
        public static FileServicesOptions AddInfluxDbStore(this FileServicesOptions app, Func<System.IServiceProvider, InfluxDbStore> factory)
        {
            return app.AddFileStore(sp => factory(sp));
        }
    }
}

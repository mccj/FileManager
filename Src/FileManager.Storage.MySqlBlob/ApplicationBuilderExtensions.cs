using FileManager.Storage.MySqlBlob;
using System;

namespace FileManager
{
    public static class MySqlBlobStoreApplicationBuilderExtensions
    {
        /// <summary>注册 MySQL BLOB 存储。</summary>
        public static FileServicesOptions AddMySqlBlobStore(this FileServicesOptions app, string connectionString)
        {
            return app.AddFileStore(_ => new MySqlBlobStore(connectionString));
        }

        /// <summary>注册 MySQL BLOB 存储。</summary>
        public static FileServicesOptions AddMySqlBlobStore(this FileServicesOptions app, Func<System.IServiceProvider, MySqlBlobStore> factory)
        {
            return app.AddFileStore(sp => factory(sp));
        }
    }
}

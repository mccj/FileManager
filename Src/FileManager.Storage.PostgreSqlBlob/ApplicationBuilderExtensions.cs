using FileManager.Storage.PostgreSqlBlob;
using System;

namespace FileManager
{
    public static class PostgreSqlBlobStoreApplicationBuilderExtensions
    {
        /// <summary>注册 PostgreSQL BLOB 存储。</summary>
        public static FileServicesOptions AddPostgreSqlBlobStore(this FileServicesOptions app, string connectionString)
        {
            return app.AddFileStore(_ => new PostgreSqlBlobStore(connectionString));
        }

        /// <summary>注册 PostgreSQL BLOB 存储。</summary>
        public static FileServicesOptions AddPostgreSqlBlobStore(this FileServicesOptions app, Func<System.IServiceProvider, PostgreSqlBlobStore> factory)
        {
            return app.AddFileStore(sp => factory(sp));
        }
    }
}

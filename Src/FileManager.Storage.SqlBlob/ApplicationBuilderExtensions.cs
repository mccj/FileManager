using FileManager.Storage.SqlBlob;
using System;

namespace FileManager
{
    public static class SqlBlobStoreApplicationBuilderExtensions
    {
        /// <summary>注册 SQLite BLOB 存储。</summary>
        public static FileServicesOptions AddSqlBlobStore(this FileServicesOptions app, string connectionString)
        {
            return app.AddFileStore(_ => new SqlBlobStore(connectionString));
        }

        /// <summary>注册 SQLite BLOB 存储。</summary>
        public static FileServicesOptions AddSqlBlobStore(this FileServicesOptions app, Func<System.IServiceProvider, SqlBlobStore> factory)
        {
            return app.AddFileStore(sp => factory(sp));
        }
    }
}

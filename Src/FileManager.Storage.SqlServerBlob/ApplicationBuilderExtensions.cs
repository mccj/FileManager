using FileManager.Storage.SqlServerBlob;
using System;

namespace FileManager
{
    public static class SqlServerBlobStoreApplicationBuilderExtensions
    {
        /// <summary>注册 SQL Server BLOB 存储。</summary>
        public static FileServicesOptions AddSqlServerBlobStore(this FileServicesOptions app, string connectionString)
        {
            return app.AddFileStore(_ => new SqlServerBlobStore(connectionString));
        }

        /// <summary>注册 SQL Server BLOB 存储。</summary>
        public static FileServicesOptions AddSqlServerBlobStore(this FileServicesOptions app, Func<System.IServiceProvider, SqlServerBlobStore> factory)
        {
            return app.AddFileStore(sp => factory(sp));
        }
    }
}

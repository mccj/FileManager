using FileManager.Storage.Cassandra;
using System;

namespace FileManager
{
    public static class CassandraStoreApplicationBuilderExtensions
    {
        /// <summary>注册 Cassandra 对象存储。</summary>
        public static FileServicesOptions AddCassandraStore(this FileServicesOptions app, string contactPoints, string keyspace)
        {
            return app.AddFileStore(_ => new CassandraStore(contactPoints, keyspace));
        }

        /// <summary>注册 Cassandra 对象存储。</summary>
        public static FileServicesOptions AddCassandraStore(this FileServicesOptions app, Func<System.IServiceProvider, CassandraStore> factory)
        {
            return app.AddFileStore(sp => factory(sp));
        }
    }
}

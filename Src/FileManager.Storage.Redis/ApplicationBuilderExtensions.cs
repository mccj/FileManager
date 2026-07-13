using FileManager.Storage.Redis;
using StackExchange.Redis;
using System;

namespace FileManager
{
    public static class RedisStoreApplicationBuilderExtensions
    {
        /// <summary>注册 Redis 键值文件存储。</summary>
        public static FileServicesOptions AddRedisStore(this FileServicesOptions app, string connectionString, int database = -1)
        {
            return app.AddFileStore(_ => new RedisStore(connectionString, database));
        }

        /// <summary>注册 Redis 键值文件存储。</summary>
        public static FileServicesOptions AddRedisStore(this FileServicesOptions app, Func<System.IServiceProvider, RedisStore> factory)
        {
            return app.AddFileStore(sp => factory(sp));
        }

        /// <summary>注册 Redis 键值文件存储。</summary>
        public static FileServicesOptions AddRedisStore(this FileServicesOptions app, IConnectionMultiplexer connection, int database = -1)
        {
            return app.AddFileStore(_ => new RedisStore(connection, database));
        }
    }
}

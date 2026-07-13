using System;
using FileManager.Storage.MongoDB;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace FileManager
{
    /// <summary>
    /// MongoDB GridFS 存储注册扩展。
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>注册 MongoDB GridFS 存储。</summary>
        public static FileServicesOptions AddMongoDBStore(this FileServicesOptions app, System.Func<System.IServiceProvider, MongoDBStore> func)
        {
            app.Services.AddSingleton<FileStorage.IFileStore>((sp) =>
            {
                return func(sp);
            });

            return app;
        }
        /// <summary>注册 MongoDB GridFS 存储。</summary>
        public static FileServicesOptions AddMongoDBStore(this FileServicesOptions app, string url)
        {
            return AddMongoDBStore(app, sp => new MongoDBStore(url));
        }
        /// <summary>注册 MongoDB GridFS 存储。</summary>
        public static FileServicesOptions AddMongoDBStore(this FileServicesOptions app, MongoUrl url)
        {
            return AddMongoDBStore(app, sp => new MongoDBStore(url));
        }
        /// <summary>注册 MongoDB GridFS 存储。</summary>
        public static FileServicesOptions AddMongoDBStore(this FileServicesOptions app, MongoClient client)
        {
            return AddMongoDBStore(app, sp => new MongoDBStore(client));
        }
    }
}

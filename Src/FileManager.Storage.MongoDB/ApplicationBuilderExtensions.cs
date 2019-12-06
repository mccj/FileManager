using System;
using FileManager.Storage.MongoDB;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace FileManager
{
    /// <summary>
    /// 
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        public static FileServicesOptions AddMongoDBStore(this FileServicesOptions app, System.Func<System.IServiceProvider, MongoDBStore> func)
        {
            app.Services.AddSingleton<FileStorage.IFileStore>((sp) =>
            {
                return func(sp);
            });

            return app;
        }
        public static FileServicesOptions AddMongoDBStore(this FileServicesOptions app, string url)
        {
            return AddMongoDBStore(app, sp => new MongoDBStore(url));
        }
        public static FileServicesOptions AddMongoDBStore(this FileServicesOptions app, MongoUrl url)
        {
            return AddMongoDBStore(app, sp => new MongoDBStore(url));
        }
        public static FileServicesOptions AddMongoDBStore(this FileServicesOptions app, MongoClient client)
        {
            return AddMongoDBStore(app, sp => new MongoDBStore(client));
        }
    }
}

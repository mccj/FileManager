using FileManager.Storage.Elasticsearch;
using System;

namespace FileManager
{
    public static class ElasticsearchStoreApplicationBuilderExtensions
    {
        /// <summary>注册 Elasticsearch 文档存储。</summary>
        public static FileServicesOptions AddElasticsearchStore(this FileServicesOptions app, Uri endpoint, string indexName = "fm_nodes")
        {
            return app.AddFileStore(_ => new ElasticsearchStore(endpoint, indexName));
        }

        /// <summary>注册 Elasticsearch 文档存储。</summary>
        public static FileServicesOptions AddElasticsearchStore(this FileServicesOptions app, Func<System.IServiceProvider, ElasticsearchStore> factory)
        {
            return app.AddFileStore(sp => factory(sp));
        }
    }
}

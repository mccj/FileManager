using FileManager.Storage.SharePoint;
using System;
using System.Net.Http;

namespace FileManager
{
    public static class SharePointStoreApplicationBuilderExtensions
    {
        /// <summary>注册 SharePoint 文档库存储。</summary>
        public static FileServicesOptions AddSharePointStore(this FileServicesOptions app, string siteUrl, string accessToken)
        {
            return app.AddFileStore(_ => new SharePointStore(siteUrl, accessToken));
        }

        /// <summary>注册 SharePoint 文档库存储。</summary>
        public static FileServicesOptions AddSharePointStore(this FileServicesOptions app, string siteUrl, string accessToken, HttpClient httpClient)
        {
            return app.AddFileStore(_ => new SharePointStore(siteUrl, accessToken, httpClient));
        }

        /// <summary>注册 SharePoint 文档库存储。</summary>
        public static FileServicesOptions AddSharePointStore(this FileServicesOptions app, Func<System.IServiceProvider, SharePointStore> factory)
        {
            return app.AddFileStore(sp => factory(sp));
        }
    }
}

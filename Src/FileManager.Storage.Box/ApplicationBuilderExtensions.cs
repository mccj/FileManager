using FileManager.Storage.Box;

using System;

using System.Net.Http;



namespace FileManager

{

    public static class BoxStoreApplicationBuilderExtensions

    {

        /// <summary>注册 Box 云存储。</summary>
        public static FileServicesOptions AddBoxStore(this FileServicesOptions app, string accessToken)

        {

            return app.AddFileStore(_ => new BoxStore(accessToken));

        }



        /// <summary>注册 Box 云存储。</summary>
        public static FileServicesOptions AddBoxStore(this FileServicesOptions app, string accessToken, HttpClient httpClient)

        {

            return app.AddFileStore(_ => new BoxStore(accessToken, httpClient));

        }



        /// <summary>注册 Box 云存储。</summary>
        public static FileServicesOptions AddBoxStore(this FileServicesOptions app, Func<System.IServiceProvider, BoxStore> factory)

        {

            return app.AddFileStore(sp => factory(sp));

        }

    }

}


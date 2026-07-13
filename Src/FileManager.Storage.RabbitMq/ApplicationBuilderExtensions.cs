using FileManager.Storage.RabbitMq;

using RabbitMQ.Client;

using System;



namespace FileManager

{

    public static class RabbitMqStoreApplicationBuilderExtensions

    {

        /// <summary>注册 RabbitMQ 消息队列实验性存储。</summary>
        public static FileServicesOptions AddRabbitMqStore(this FileServicesOptions app, string connectionString)

        {

            return app.AddFileStore(_ => new RabbitMqStore(connectionString));

        }



        /// <summary>注册 RabbitMQ 消息队列实验性存储。</summary>
        public static FileServicesOptions AddRabbitMqStore(this FileServicesOptions app, IConnection connection)

        {

            return app.AddFileStore(_ => new RabbitMqStore(connection));

        }



        /// <summary>注册 RabbitMQ 消息队列实验性存储。</summary>
        public static FileServicesOptions AddRabbitMqStore(this FileServicesOptions app, Func<System.IServiceProvider, RabbitMqStore> factory)

        {

            return app.AddFileStore(sp => factory(sp));

        }

    }

}


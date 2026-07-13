using FileManager.Storage.Kafka;

using System;



namespace FileManager

{

    public static class KafkaStoreApplicationBuilderExtensions

    {

        /// <summary>注册 Kafka 主题实验性存储。</summary>
        public static FileServicesOptions AddKafkaStore(this FileServicesOptions app, string bootstrapServers, string topic = "fm-files")

        {

            return app.AddFileStore(_ => new KafkaStore(bootstrapServers, topic));

        }



        /// <summary>注册 Kafka 主题实验性存储。</summary>
        public static FileServicesOptions AddKafkaStore(this FileServicesOptions app, Func<System.IServiceProvider, KafkaStore> factory)

        {

            return app.AddFileStore(sp => factory(sp));

        }

    }

}


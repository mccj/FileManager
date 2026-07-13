using FileManager;
using FileManager.Storage.Compress;
using System;
using System.Configuration;
using System.IO;

namespace FileManager.Sample.Owin
{
    /// <summary>
    /// OWIN 示例支持的存储后端（Physical、Compress）。配置来源：App.config 或环境变量。
    /// </summary>
    internal static class OwinSampleStorage
    {
        /// <summary>当前存储 Provider 名称。</summary>
        public static string CurrentProvider =>
            (Environment.GetEnvironmentVariable("FileManager__Storage__Provider")
             ?? ConfigurationManager.AppSettings["FileManager:Storage:Provider"]
             ?? "Physical").Trim();

        /// <summary>将选定的存储注册到 FileServicesOptions。</summary>
        public static void Configure(FileServicesOptions options)
        {
            switch (CurrentProvider.ToLowerInvariant())
            {
                case "physical":
                    {
                        var path = GetSetting("FileManager:Storage:Physical:Path", "App_Data\\Files");
                        var root = OwinApplicationBuilderExtensions.ResolveContentPath(path);
                        Directory.CreateDirectory(root);
                        options.AddFileProviderPhysicalPathReadOnly(root);
                    }
                    break;
                case "compress":
                    {
                        var archivePath = GetSetting("FileManager:Storage:Compress:ArchivePath", "App_Data\\sample.zip");
                        var fullPath = OwinApplicationBuilderExtensions.ResolveContentPath(archivePath);
                        if (!File.Exists(fullPath))
                            throw new FileNotFoundException($"压缩包示例文件不存在: {fullPath}", fullPath);
                        options.AddCompressStore(_ => new CompressStore(File.OpenRead(fullPath)));
                    }
                    break;
                default:
                    throw new InvalidOperationException(
                        $"OWIN 示例不支持存储提供程序「{CurrentProvider}」。请使用 Physical 或 Compress；" +
                        "InMemory、MultiDrive 与云存储请在 ASP.NET Core *·Full 示例中体验。");
            }
        }

        /// <summary>本宿主可切换的存储提供程序列表。</summary>
        public static string[] SupportedProviders { get; } = { "Physical", "Compress" };

        private static string GetSetting(string key, string defaultValue)
        {
            var value = ConfigurationManager.AppSettings[key];
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
        }
    }
}

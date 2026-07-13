using Microsoft.Owin.Hosting;
using System;

namespace FileManager.Sample.Owin
{
    /// <summary>
    /// .NET Framework 4.8 + OWIN 自宿主入口。完整多 UI / 云存储演示请使用 *·Full（ASP.NET Core）。
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            // 默认 http://localhost:8080/；存储见 App.config 或环境变量 FileManager__Storage__Provider
            var url = args.Length > 0 ? args[0] : "http://localhost:8080/";
            if (!url.EndsWith("/"))
                url += "/";

            var startOpts = new StartOptions(url);
            using (WebApp.Start<Startup>(startOpts))
            {
                Console.WriteLine("FileManager OWIN 宿主运行于 " + url);
                Console.WriteLine("当前存储: " + OwinSampleStorage.CurrentProvider);
                Console.WriteLine("打开 " + url.TrimEnd('/') + "/ （导航页）或 /Explorer/");
                Console.WriteLine("按 Enter 退出。");
                Console.ReadLine();
            }
        }
    }
}

using Microsoft.Owin.Hosting;
using System;

namespace ConsoleOwinTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var url = args.Length > 0 ? args[0] : "http://localhost:8080/";
            if (!url.EndsWith("/"))
                url += "/";

            var startOpts = new StartOptions(url);
            using (WebApp.Start<Startup>(startOpts))
            {
                Console.WriteLine("FileManager OWIN host running at " + url);
                Console.WriteLine("Open " + url.TrimEnd('/') + "/Browser/");
                Console.WriteLine("Press Enter to exit.");
                Console.ReadLine();
            }
        }
    }
}

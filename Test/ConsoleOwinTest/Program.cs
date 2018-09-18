using Microsoft.Owin.Hosting;
using System;

namespace ConsoleOwinTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var url = "http://localhost:8080/";
            var startOpts = new StartOptions(url)
            {
                
            };
            using (WebApp.Start<Startup>(startOpts))
            {
                Console.WriteLine("Server run at " + url + " \r\npress Enter to exit.");
                Console.ReadLine();
            }
        }
    }
}

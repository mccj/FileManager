using Microsoft.Owin;
using Owin;
using System.Threading.Tasks;
using FileManager;

namespace ConsoleOwinTest
{
    public class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            appBuilder.UseFileManager("/hangfire", "D:\\test\\ContentLibrary");

            appBuilder.Run(HandleRequest);

            appBuilder.Use((context, next) =>
            {
                return next.Invoke();
            });
        }

        static Task HandleRequest(IOwinContext context)
        {
            //context.Request.ReadFormAsync().Result.
            //OwinRequest
            context.Response.ContentType = "text/plain";
            return context.Response.WriteAsync("Hello, world!");
        }



        //internal static void ParseDelimited(string text, char[] delimiters, System.Action<string, string, object> callback, object state)
        //{
        //    int textLength = text.Length;
        //    int equalIndex = text.IndexOf('=');
        //    if (equalIndex == -1)
        //    {
        //        equalIndex = textLength;
        //    }
        //    int scanIndex = 0;
        //    while (scanIndex < textLength)
        //    {
        //        int delimiterIndex = text.IndexOfAny(delimiters, scanIndex);
        //        if (delimiterIndex == -1)
        //        {
        //            delimiterIndex = textLength;
        //        }
        //        if (equalIndex < delimiterIndex)
        //        {
        //            while (scanIndex != equalIndex && char.IsWhiteSpace(text[scanIndex]))
        //            {
        //                ++scanIndex;
        //            }
        //            string name = text.Substring(scanIndex, equalIndex - scanIndex);
        //            string value = text.Substring(equalIndex + 1, delimiterIndex - equalIndex - 1);
        //            callback(
        //                System.Uri.UnescapeDataString(name.Replace('+', ' ')),
        //                System.Uri.UnescapeDataString(value.Replace('+', ' ')),
        //                state);
        //            equalIndex = text.IndexOf('=', delimiterIndex);
        //            if (equalIndex == -1)
        //            {
        //                equalIndex = textLength;
        //            }
        //        }
        //        scanIndex = delimiterIndex + 1;
        //    }
        //}
    }
}

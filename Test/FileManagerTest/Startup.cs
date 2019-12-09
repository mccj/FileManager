using FileManager;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FileManagerTest
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
               Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddFileManager(f => f
            //.AddWebRootFileProviderReadOnly()
            .AddContentRootFileProviderReadOnly()
            //.AddFileProviderRootPhysicalPathReadOnly("/bin")
            //.AddFileProviderPhysicalPathReadOnly("d://")
            //.AddRootPhysicalFilePath("/bin")
            //.AddPhysicalFilePath("d://")
            //.AddFtpStore("ftp://10.11.1.15/", 21, "mccj", "123")
            //.AddWebDavStore(new System.Uri("http://10.11.11.11/remote.php/webdav/"), "mccj", "123")
            //.AddCompressStore(System.IO.File.OpenRead(@"D:\Users\mccj\Source\Repos\FileManager\Test\FileManagerTest\wwwroot\AspNetCore-2.2.6.zip"))
            );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseFileManagerUI("/Browser", f => f
                 //.UseDefaultFileManagerUI()
                 .UseFileExplorerUI()
            );

            //app.UseFileManagerUI("/RichFileManagerBrowser", f => f
            //    .UseRichFileManagerUI()
            //);
            //app.UseFileManager("/hangfire"/*, "D:\\test\\ContentLibrary"*/);

            //app.UseFileManager("/hangfire", "D:\\test\\ContentLibrary");
            //app.UseFileManagerForFtp("/hangfire", "ftp://10.11.1.15/",21,"admin","admin"/*, "D:\\test\\ContentLibrary"*/);

            app.UseMvc();
        }
    }
}

using FileManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SwaggerExtensions;

#if netcoreapp3
using IWebHostEnvironment = Microsoft.AspNetCore.Hosting.IWebHostEnvironment;
#else
using IWebHostEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
#endif

namespace FileManagerCore3
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
#if netcoreapp3
            services.AddControllersWithViews();
#else
            services.AddMvc();
#endif

            services.AddFileManager(f => f
            //.AddWebRootFileProviderReadOnly()
            //.AddFileProviderRootPhysicalPathReadOnly("/bin")
            //.AddFileProviderPhysicalPathReadOnly("d://")
            //.AddRootPhysicalFilePath("/bin")
            //.AddPhysicalFilePath("d://")
            .AddRootPhysicalFilePath("/App_Data/Files")
            //.AddMongoDBStore("mongodb://10.11.16.11:27017/")
            //.AddFtpStore("ftp://10.11.1.15/", 21, "mccj", "123")
            //.AddWebDavStore(new System.Uri("http://10.11.11.11/remote.php/webdav/"), "mccj", "123")
            //.AddCompressStore(System.IO.File.OpenRead(@"D:\Users\mccj\Source\Repos\FileManager\Test\FileManagerTest\wwwroot\AspNetCore-2.2.6.zip"))
            //.AddPermissionHandle((sp, f) => Task.FromResult(FileManager.FileStorage.Permission.Compress))
            );
            services.AddNSwagSwagger();
            services.AddHealthChecks();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();
            app.UseNSwagSwaggerUI();
            app.UseHealthChecks("/health");
            app.UseFileManagerUI("/Browser", f => f
                 //.UseDefaultFileManagerUI()
                 .UseFileExplorerUI()
            );

#if netcoreapp3
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
#else
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
#endif
        }
    }
}

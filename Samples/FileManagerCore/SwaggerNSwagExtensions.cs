//using Microsoft.AspNetCore.Builder;
//using Microsoft.Extensions.DependencyInjection;
//using Newtonsoft.Json;
//using NSwag.Annotations;
//using NSwag.AspNetCore;
//using NSwag.Generation.AspNetCore;
//using NSwag.Generation.Processors.Contexts;
//using System.Collections.Generic;
//using System.Linq;

//namespace SwaggerExtensions
//{
//    public static class SwaggerNSwagExtensions
//    {
//        public static IServiceCollection AddNSwagSwagger(this IServiceCollection services)
//        {

//            services
//                // Register an OpenAPI 3.0 document generator
//                .AddOpenApiDocument((document, sp) =>
//                {
//                    _settings(document);
//                    document.DocumentName = "openapi/" + document.Version;
//                })
//                // Register a Swagger 2.0 document generator
//                .AddSwaggerDocument((document, sp) =>
//                {
//                    _settings(document);
//                    document.DocumentName = "swagger/" + document.Version;
//                })
//            ;
//            return services;
//        }
//        public static IApplicationBuilder UseNSwagSwaggerUI(this IApplicationBuilder app)
//        {
//            UseNSwagSwagger(app);
//            UseNSwagSwaggerUI3(app);
//            UseNSwagReDoc(app);
//            return app;
//        }
//        private static IApplicationBuilder UseNSwagReDoc(this IApplicationBuilder app, string path = null)
//        {
//            // Config with support for multiple documents
//            var swaggerDocumentRegistration = app.ApplicationServices.GetServices<OpenApiDocumentRegistration>();
//            foreach (var item in swaggerDocumentRegistration)
//            {
//                app.UseReDoc(config =>
//                {
//                    if (!string.IsNullOrWhiteSpace(path))
//                        config.Path = path;
//                    config.DocumentPath = config.DocumentPath.Replace("{documentName}", item.DocumentName);
//                    config.Path = config.Path + "/ReDoc";

//                    config.TransformToExternalPath = (internalUiRoute, request) =>
//                    {
//                        // The header X-External-Path is set in the nginx.conf file
//                        var externalPath = request.Headers.ContainsKey("X-External-Path") ? request.Headers["X-External-Path"].First() : "";
//                        return externalPath + internalUiRoute;
//                    };
//                })
//                ;
//            }


//            // Config with single document
//            //app.UseReDoc(config =>
//            //{
//            //    config.SwaggerRoute = "/swagger/v1/swagger.json";
//            //    config.TransformToExternalPath = (internalUiRoute, request) =>
//            //    {
//            //        // The header X-External-Path is set in the nginx.conf file
//            //        var externalPath = request.Headers.ContainsKey("X-External-Path") ? request.Headers["X-External-Path"].First() : "";
//            //        return externalPath + internalUiRoute;
//            //    };
//            //});


//            return app;
//        }

//        private static IApplicationBuilder UseNSwagSwaggerUI3(this IApplicationBuilder app, string path = null)
//        {
//            // Config with support for multiple documents
//            app.UseSwaggerUi3(config =>
//            {
//                if (!string.IsNullOrWhiteSpace(path))
//                    config.Path = path;
//                config.TransformToExternalPath = (internalUiRoute, request) =>
//                {
//                    // The header X-External-Path is set in the nginx.conf file
//                    var externalPath = request.Headers.ContainsKey("X-External-Path") ? request.Headers["X-External-Path"].First() : "";
//                    return externalPath + internalUiRoute;
//                };
//            });

//            return app;
//        }

//        private static IApplicationBuilder UseNSwagSwagger(IApplicationBuilder app, string path = null)
//        {
//            app.UseOpenApi(config =>
//            {
//                if (!string.IsNullOrWhiteSpace(path))
//                    config.Path = path;

//                config.PostProcess = (document, request) =>
//                {
//                    if (request.Headers.ContainsKey("X-External-Host"))
//                    {
//                        // Change document server settings to public
//                        document.Host = request.Headers["X-External-Host"].First();
//                        document.BasePath = request.Headers["X-External-Path"].First();
//                    }
//                };
//            });
//            return app;
//        }

//        private static void _settings(AspNetCoreOpenApiDocumentGeneratorSettings document, string version = "v1", string state = "(稳定版)")
//        {
//            document.Title = "天使项目 API 文档" + state;
//            document.Description = @"天使项目 API 文档,可以使用API Key来授权测试。

//# Introduction
//This API is documented in **OpenAPI format** and is based on

//# Authentication

// Petstore offers two forms of authentication:
//      - API Key
//      - OAuth2
//    OAuth2 - an open protocol to allow secure authorization in a simple
//    and standard method from web, mobile and desktop applications.

//";
//            document.Version = version;


//            document.PostProcess = (f) =>
//            {
//                f.Info.TermsOfService = "http://www.weberp.com.cn";
//                f.Info.Contact = new NSwag.OpenApiContact { Email = "mccj@weberp.com.cn", Name = "The KeWei Team", Url = "http://www.weberp.com.cn" };
//                f.Info.License = new NSwag.OpenApiLicense { Name = "Apache 2.0", Url = "http://www.apache.org/licenses/LICENSE-2.0.html" };

//                f.Info.ExtensionData = new Dictionary<string, object>();
//                f.Info.ExtensionData.Add("x-logo", new { url = "https://rebilly.github.io/ReDoc/petstore-logo.png", altText = "Petstore logo" });
//            };


//            //document.DefaultEnumHandling = NJsonSchema.EnumHandling.CamelCaseString;
//            //document.DefaultPropertyNameHandling = NJsonSchema.PropertyNameHandling.CamelCase;
//            ////document.DefaultReferenceTypeNullHandling = NJsonSchema.ReferenceTypeNullHandling.Null;

//        }
//    }

//    public class ReDocCodeSampleAttribute : OpenApiOperationProcessorAttribute
//    {
//        public ReDocCodeSampleAttribute(string language, string source)
//            : base(typeof(ReDocCodeSampleAppender), language, source)
//        {
//        }

//        internal class ReDocCodeSampleAppender : NSwag.Generation.Processors.IOperationProcessor
//        {
//            private readonly string _language;
//            private readonly string _source;
//            private const string ExtensionKey = "x-code-samples";

//            public ReDocCodeSampleAppender(string language, string source)
//            {
//                //var document = NSwag.SwaggerDocument.FromJsonAsync("...").Result;
//                //var settings = new NSwag.CodeGeneration.CSharp.SwaggerToCSharpClientGeneratorSettings
//                //{
//                //    ClassName = "MyClass",
//                //    CSharpGeneratorSettings =
//                //    {
//                //        Namespace = "MyNamespace"
//                //    }
//                //};
//                //var generator = new NSwag.CodeGeneration.CSharp.SwaggerToCSharpClientGenerator(document, settings);



//                _language = language;
//                _source = source;
//            }

//            public bool Process(OperationProcessorContext context)
//            {
//                if (context.OperationDescription.Operation.ExtensionData == null)
//                    context.OperationDescription.Operation.ExtensionData = new Dictionary<string, object>();

//                var data = context.OperationDescription.Operation.ExtensionData;
//                if (!data.ContainsKey(ExtensionKey))
//                    data[ExtensionKey] = new List<ReDocCodeSample>();

//                var samples = (List<ReDocCodeSample>)data[ExtensionKey];
//                samples.Add(new ReDocCodeSample
//                {
//                    Language = _language,
//                    Source = _source,
//                });

//                return true;
//            }
//        }

//        internal class ReDocCodeSample
//        {
//            [JsonProperty("lang")]
//            public string Language { get; set; }

//            [JsonProperty("source")]
//            public string Source { get; set; }
//        }
//    }
//}

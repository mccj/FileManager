using Microsoft.AspNetCore.Mvc.Testing;

using System.Net;

using Xunit;

using MvcFullHosting = FileManager.Sample.Mvc.Full.HostingEntryPoint;



namespace FileManager.Samples.Test;



public class MvcFullSampleSmokeTests : IClassFixture<WebApplicationFactory<MvcFullHosting>>

{

    private readonly HttpClient _client;



    public MvcFullSampleSmokeTests(WebApplicationFactory<MvcFullHosting> factory)

    {

        _client = factory.CreateClient(new WebApplicationFactoryClientOptions

        {

            AllowAutoRedirect = false

        });

    }



    [Theory]

    [InlineData("/")]

    [InlineData("/Explorer/")]

    [InlineData("/health")]

    [InlineData("/swagger/index.html")]

    public async Task Endpoints_ReturnSuccess(string path)

    {

        var response = await _client.GetAsync(path);

        Assert.True(

            response.StatusCode is HttpStatusCode.OK or HttpStatusCode.Redirect or HttpStatusCode.MovedPermanently,

            $"GET {path} returned {(int)response.StatusCode}");

    }

}



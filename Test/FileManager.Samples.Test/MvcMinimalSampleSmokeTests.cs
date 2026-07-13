using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Xunit;
using MvcMinimalHosting = FileManager.Sample.Mvc.Minimal.HostingEntryPoint;

namespace FileManager.Samples.Test;

public class MvcMinimalSampleSmokeTests : IClassFixture<WebApplicationFactory<MvcMinimalHosting>>
{
    private readonly HttpClient _client;

    public MvcMinimalSampleSmokeTests(WebApplicationFactory<MvcMinimalHosting> factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/Browser/")]
    [InlineData("/Default/")]
    public async Task Endpoints_ReturnSuccess(string path)
    {
        var response = await _client.GetAsync(path);
        Assert.True(
            response.StatusCode is HttpStatusCode.OK or HttpStatusCode.Redirect or HttpStatusCode.MovedPermanently,
            $"GET {path} returned {(int)response.StatusCode}");
    }
}

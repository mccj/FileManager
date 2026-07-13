using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Xunit;
using BlazorFullHosting = FileManager.Sample.Blazor.Full.HostingEntryPoint;

namespace FileManager.Samples.Test;

public class BlazorFullSampleSmokeTests : IClassFixture<WebApplicationFactory<BlazorFullHosting>>
{
    private readonly HttpClient _client;

    public BlazorFullSampleSmokeTests(WebApplicationFactory<BlazorFullHosting> factory)
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

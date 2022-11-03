using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc.Testing;

using RobotsInc.Inspections.API.I;

using Xunit;

namespace RobotsInc.Inspections.Host.API.I;

public class HealthControllerTests : ControllerTests
{
    [Fact]
    public async Task health_is_healthy_within_office_hours()
    {
        var signInfo = GenerateRandomKeys(true);
        DateTime dateTime = new(2022, 3, 29, 10, 16, 0, DateTimeKind.Local);
        await using WebApplicationFactory<Program> factory = GenerateWebApplicationFactory(UseMockTimeProvider(dateTime), UseConnectionString(), UseMockJwtToken(signInfo.ValidateKey));
        await InitializeDatabaseAsync(factory.Services);
        HttpClient client = factory.CreateClient();
        HttpResponseMessage response = await client.GetAsync(Routes.ApiV1 + Routes.Health);
        Assert.NotNull(response);
        Assert.StrictEqual(HttpStatusCode.OK, response.StatusCode);
        await TearDownDatabaseAsync(factory.Services);
    }

    [Fact]
    public async Task health_is_closed_outside_of_office_hours()
    {
        var signInfo = GenerateRandomKeys(false);
        DateTime unhealthyDateTime = new(2022, 3, 29, 17, 25, 0, DateTimeKind.Local);
        await using WebApplicationFactory<Program> factory = GenerateWebApplicationFactory(UseMockTimeProvider(unhealthyDateTime), UseConnectionString(), UseMockJwtToken(signInfo.ValidateKey));
        await InitializeDatabaseAsync(factory.Services);
        HttpClient client = factory.CreateClient();
        HttpResponseMessage response = await client.GetAsync(Routes.ApiV1 + Routes.Health);
        Assert.NotNull(response);
        Assert.StrictEqual(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        await TearDownDatabaseAsync(factory.Services);
    }
}

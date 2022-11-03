using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RobotsInc.Inspections.API.I;
using RobotsInc.Inspections.API.I.Health;
using RobotsInc.Inspections.API.I.Json;

namespace RobotsInc.Inspections.Client;

/// <summary>
///     Single <see cref="IHostedService" /> for a console application.
/// </summary>
public class ConsoleHostedService : BackgroundService
{
    private readonly InspectionsApiOptions _inspectionsApiOptions;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ConsoleHostedService> _logger;

    public ConsoleHostedService(
        IOptions<InspectionsApiOptions> inspectionsApiOptions,
        ILogger<ConsoleHostedService> logger,
        IHostApplicationLifetime hostApplicationLifetime,
        IHttpClientFactory httpClientFactory)
    {
        _inspectionsApiOptions = inspectionsApiOptions.Value;
        _logger = logger;
        _hostApplicationLifetime = hostApplicationLifetime;
        _httpClientFactory = httpClientFactory;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        const string HealthPath = Routes.ApiV1 + Routes.Health;
        JsonSerializerOptions serializerOptions =
            new(JsonSerializerDefaults.Web)
            {
                WriteIndented = true,
                AllowTrailingCommas = false,
                NumberHandling = JsonNumberHandling.Strict,
                Converters =
                {
                    new JsonStringEnumConverter(null, false),
                    new RobotJsonConverter()
                }
            };

        // don't block during start of IHostedService
        await Task.Yield();

        // execute
        try
        {
            _logger.LogInformation($"{nameof(ConsoleHostedService)} started execution.");

            using HttpClient httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(_inspectionsApiOptions.BaseAddress);
            httpClient.Timeout = _inspectionsApiOptions.Timeout;

            using HttpRequestMessage request =
                new()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(HealthPath, UriKind.Relative),
                };

            using HttpResponseMessage response =
                await httpClient.SendAsync(request, stoppingToken);

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync(stoppingToken);
                HealthResult? health = JsonSerializer.Deserialize<HealthResult>(json, serializerOptions);
                Console.WriteLine($"Call succeeded with status code: {response.StatusCode.ToString()}");
                Console.WriteLine("Response body:");
                Console.WriteLine(json);
                if (health != null)
                {
                    Console.WriteLine($"Status  : {health.Status}");
                    Console.WriteLine($"Message : {health.Message}");
                }
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                Console.WriteLine($"Call failed with status code: {response.StatusCode.ToString()}");
                Console.WriteLine($"The endpoint was not found on the path: {HealthPath}");
            }
            else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                string json = await response.Content.ReadAsStringAsync(stoppingToken);
                HealthResult? health = JsonSerializer.Deserialize<HealthResult>(json, serializerOptions);
                Console.WriteLine($"Service is not available, answer with status code: {response.StatusCode.ToString()}");
                Console.WriteLine("Response body:");
                Console.WriteLine(json);
                if (health != null)
                {
                    Console.WriteLine($"Status  : {health.Status}");
                    Console.WriteLine($"Message : {health.Message}");
                }
            }
            else
            {
                Console.WriteLine($"Unexpected status code: {response.StatusCode.ToString()}");
                string json = await response.Content.ReadAsStringAsync(stoppingToken);
                Console.WriteLine("Response body:");
                Console.WriteLine(json);
            }

            _logger.LogInformation($"{nameof(ConsoleHostedService)} stopped execution.");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Aborted because of error.");
        }
        finally
        {
            _hostApplicationLifetime.StopApplication();
        }
    }
}

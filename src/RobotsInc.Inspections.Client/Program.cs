using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace RobotsInc.Inspections.Client;

public static class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine($"Running {typeof(Program).Namespace}");

        IHostBuilder? hostBuilder =
            Host
                .CreateDefaultBuilder(args)
                .ConfigureServices(
                    (context, services) =>
                    {
                        // configuration options
                        IConfigurationSection apiSection =
                            context.Configuration.GetSection(InspectionsApiOptions.Key);
                        services
                            .Configure<InspectionsApiOptions>(apiSection);

                        // http client factory
                        services
                            .AddHttpClient();

                        // own hosted service
                        services
                            .AddHostedService<ConsoleHostedService>();
                    });

        await hostBuilder.RunConsoleAsync();
    }
}
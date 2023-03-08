using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using Hellang.Authentication.JwtBearer.Google;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

using RobotsInc.Inspections.API.I.Json;
using RobotsInc.Inspections.BusinessLogic.Util;
using RobotsInc.Inspections.Repositories;

namespace RobotsInc.Inspections.Host;

public class ControllerTests
{
    public Action<IServiceCollection> UseMockTimeProvider(DateTime datetime)
        => services =>
           {
               services.Remove(services.Single(s => s.ServiceType == typeof(ITimeProvider)));
               services.AddSingleton<ITimeProvider>(new MockTimeProvider(datetime));
           };

    public Action<IServiceCollection> UseConnectionString([CallerMemberName] string method = "default")
        => services =>
           {
               services.Remove(services.Single(s => s.ServiceType == typeof(DbContextOptions<InspectionsDbContext>)));
               services.AddDbContext<InspectionsDbContext>(
                   (sp, options) =>
                   {
                       IConfiguration configuration = sp.GetRequiredService<IConfiguration>();
                       string connectionString = configuration.GetConnectionString("InspectionsTest");
                       SqlConnectionStringBuilder sqlConnectionStringBuilder = new(connectionString);
                       sqlConnectionStringBuilder.InitialCatalog += $"-{GetType().Name}-{method}";

                       options
                           .UseLazyLoadingProxies()
                           .EnableDetailedErrors()
                           .EnableSensitiveDataLogging()
                           .UseSqlServer(
                               sqlConnectionStringBuilder.ConnectionString,
                               sqlServerOptions =>
                               {
                                   sqlServerOptions.MigrationsAssembly("RobotsInc.Inspections.Migrations");
                               });
                   });
           };

    public Action<IServiceCollection> UseMockJwtToken(SecurityKey key)
        => services =>
           {
               services.Configure<JwtBearerOptions>(
                   GoogleJwtBearerDefaults.AuthenticationScheme,
                   options =>
                   {
                       options.TokenValidationParameters.IssuerSigningKey = key;
                   });
           };

    public async Task InitializeDatabaseAsync(IServiceProvider serviceProvider)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        InspectionsDbContext inspectionsDbContext = scope.ServiceProvider.GetRequiredService<InspectionsDbContext>();
        await inspectionsDbContext.Database.EnsureDeletedAsync();
        await inspectionsDbContext.Database.EnsureCreatedAsync();
    }

    public async Task TearDownDatabaseAsync(IServiceProvider serviceProvider)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        InspectionsDbContext inspectionsDbContext = scope.ServiceProvider.GetRequiredService<InspectionsDbContext>();
        await inspectionsDbContext.Database.EnsureDeletedAsync();
    }

    public JsonSerializerOptions JsonSerializerOptions
        => new()
           {
               PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
               NumberHandling = JsonNumberHandling.Strict,
               WriteIndented = true,
               AllowTrailingCommas = false,
               Converters =
               {
                   new JsonStringEnumConverter(null, false),
                   new RobotJsonConverter()
               }
           };

    public WebApplicationFactory<Program> GenerateWebApplicationFactory(params Action<IServiceCollection>[] actions)
    => new WebApplicationFactory<Program>()
        .WithWebHostBuilder(
            builder =>
            {
                builder.ConfigureTestServices(
                    services =>
                    {
                        foreach (Action<IServiceCollection> action in actions)
                        {
                            action(services);
                        }
                    });
            });

    public (SecurityKey SignKey, SecurityKey ValidateKey, string SignatureAlgorithm) GenerateRandomKeys(bool asymmetric)
    {
        SecurityKey? validateKey = null;
        SecurityKey? signKey = null;
        string? signatureAlgorithm;

        if (asymmetric)
        {
            signatureAlgorithm = SecurityAlgorithms.RsaSha256Signature;

            RSACryptoServiceProvider secretKeyPair = new(2048);
            RSAParameters privateParameters = secretKeyPair.ExportParameters(true);
            signKey = new RsaSecurityKey(privateParameters);
            RSAParameters publicParameters = secretKeyPair.ExportParameters(false);
            validateKey = new RsaSecurityKey(publicParameters);
        }
        else
        {
            signatureAlgorithm = SecurityAlgorithms.HmacSha256Signature;

            string randomSecret =
                Enumerable
                    .Range(0, 5)
                    .Aggregate(new StringBuilder(), (builder, _) => builder.Append(Path.GetRandomFileName().Substring(0, 8)))
                    .ToString();
            signKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(randomSecret));
            validateKey = signKey;
        }

        return (SignKey: signKey, ValidateKey: validateKey, SignatureAlgorithm: signatureAlgorithm);
    }

    public string GenerateJwt(string username, (SecurityKey SignKey, SecurityKey ValidateKey, string SignatureAlgorithm) signInfo, IConfiguration configuration)
    {
        string clientId = configuration["Authentication:Google:ClientId"];
        SecurityTokenDescriptor tokenDescriptor =
            new()
            {
                Audience = clientId,
                Issuer = "https://accounts.google.com",
                Subject = new ClaimsIdentity(new[] { new Claim("sub", username) }),
                IssuedAt = DateTime.UtcNow,
                NotBefore = null,
                Expires = DateTime.UtcNow.AddDays(1),
                Claims =
                    new Dictionary<string, object>()
                    {
                        { "azp", clientId },
                        { "email", username },
                        { "email_verified", "true" },
                        { "at_hash", "DoesNotMatter" },
                    },
                AdditionalHeaderClaims = new Dictionary<string, object>(),
                TokenType = "JWT",
                EncryptingCredentials = null,
                CompressionAlgorithm = null,
                SigningCredentials = new SigningCredentials(signInfo.SignKey, signInfo.SignatureAlgorithm)
            };

        JwtSecurityTokenHandler tokenHandler = new();
        string? token = tokenHandler.CreateEncodedJwt(tokenDescriptor);

        return token!;
    }
}

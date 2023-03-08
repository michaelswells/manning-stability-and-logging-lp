using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using RobotsInc.Inspections.API.I;
using RobotsInc.Inspections.API.I.Security;

using Xunit;

namespace RobotsInc.Inspections.Host.API.I;

public class CustomerControllerTests : ControllerTests
{
    [Fact]
    public async Task customer_with_missing_name_gets_bad_request()
    {
        // initialize keys
        var signInfo = GenerateRandomKeys(true);

        // setup api host
        await using WebApplicationFactory<Program> factory = GenerateWebApplicationFactory(UseConnectionString(), UseMockJwtToken(signInfo.ValidateKey));
        await InitializeDatabaseAsync(factory.Services);
        HttpClient client = factory.CreateClient();

        // create user and claims - as admin user
        IConfiguration configuration = factory.Services.GetRequiredService<IConfiguration>();
        string adminUser = configuration["Authentication:AdminAccount"];
        string token = GenerateJwt(adminUser, signInfo, configuration);
        client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse($"Bearer {token}");
        User employeeUser =
            new()
            {
                Email = "employee@gmail.com"
            };
        HttpResponseMessage userResponse = await client.PostAsJsonAsync(Routes.ApiV1 + Routes.Users, employeeUser, JsonSerializerOptions);
        Assert.Equal(HttpStatusCode.Created, userResponse.StatusCode);
        User createdEmployeeUser = (await userResponse.Content.ReadFromJsonAsync<User>(JsonSerializerOptions))!;
        Claim employeeClaim =
            new()
            {
                Type = ClaimTypes.Role,
                Value = ClaimTypes.Values.RoleEmployee
            };
        HttpResponseMessage claimResponse = await client.PostAsJsonAsync(Routes.ApiV1 + Routes.Users + $"/{createdEmployeeUser.Id}" + Routes.Claims, employeeClaim, JsonSerializerOptions);
        Assert.Equal(HttpStatusCode.Created, claimResponse.StatusCode);

        // create customer - as employee user
        token = GenerateJwt(employeeUser.Email, signInfo, configuration);
        client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse($"Bearer {token}");
        Customer newCustomer =
            new()
            {
                Description = "A new customer"
            };
        HttpResponseMessage response = await client.PostAsJsonAsync(Routes.ApiV1 + Routes.Customers, newCustomer, JsonSerializerOptions);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        string body = await response.Content.ReadAsStringAsync();
        Assert.NotNull(body);
        ValidationProblemDetails? details = JsonSerializer.Deserialize<ValidationProblemDetails>(body, JsonSerializerOptions);
        Assert.NotNull(details);
        Assert.Contains(nameof(Customer.Name), details!.Errors.Keys);
        await TearDownDatabaseAsync(factory.Services);
    }

    [Fact]
    public async Task customer_with_name_is_created()
    {
        // initialize keys
        var signInfo = GenerateRandomKeys(true);

        // setup api host
        await using WebApplicationFactory<Program> factory = GenerateWebApplicationFactory(UseConnectionString(), UseMockJwtToken(signInfo.ValidateKey));
        await InitializeDatabaseAsync(factory.Services);
        HttpClient client = factory.CreateClient();

        // create user and claims - as admin user
        IConfiguration configuration = factory.Services.GetRequiredService<IConfiguration>();
        string adminUser = configuration["Authentication:AdminAccount"];
        string token = GenerateJwt(adminUser, signInfo, configuration);
        client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse($"Bearer {token}");
        User employeeUser =
            new()
            {
                Email = "employee@gmail.com"
            };
        HttpResponseMessage userResponse = await client.PostAsJsonAsync(Routes.ApiV1 + Routes.Users, employeeUser, JsonSerializerOptions);
        Assert.Equal(HttpStatusCode.Created, userResponse.StatusCode);
        User createdEmployeeUser = (await userResponse.Content.ReadFromJsonAsync<User>(JsonSerializerOptions))!;
        Claim employeeClaim =
            new()
            {
                Type = ClaimTypes.Role,
                Value = ClaimTypes.Values.RoleEmployee
            };
        HttpResponseMessage claimResponse = await client.PostAsJsonAsync(Routes.ApiV1 + Routes.Users + $"/{createdEmployeeUser.Id}" + Routes.Claims, employeeClaim, JsonSerializerOptions);
        Assert.Equal(HttpStatusCode.Created, claimResponse.StatusCode);

        // create customer - as employee user
        token = GenerateJwt(employeeUser.Email, signInfo, configuration);
        client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse($"Bearer {token}");
        Customer newCustomer =
            new()
            {
                Name = "Customer name"
            };
        string bodyX = JsonSerializer.Serialize(newCustomer, JsonSerializerOptions);
        HttpResponseMessage response = await client.PostAsJsonAsync(Routes.ApiV1 + Routes.Customers, newCustomer, JsonSerializerOptions);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        string body = await response.Content.ReadAsStringAsync();
        Assert.NotNull(body);
        Uri? location = response.Headers.Location;
        Assert.NotNull(location);
        Customer? customer = JsonSerializer.Deserialize<Customer>(body, JsonSerializerOptions);
        Assert.NotNull(customer);
        Assert.Equal(newCustomer.Name, customer!.Name);
        Assert.NotNull(customer.Id);

        await TearDownDatabaseAsync(factory.Services);
    }

    [Fact]
    public async Task customer_with_name_is_created_and_fetched_by_id()
    {
        // initialize keys
        var signInfo = GenerateRandomKeys(true);

        // setup api host
        await using WebApplicationFactory<Program> factory = GenerateWebApplicationFactory(UseConnectionString(), UseMockJwtToken(signInfo.ValidateKey));
        await InitializeDatabaseAsync(factory.Services);
        HttpClient client = factory.CreateClient();

        // create user and claims - as admin user
        IConfiguration configuration = factory.Services.GetRequiredService<IConfiguration>();
        string adminUser = configuration["Authentication:AdminAccount"];
        string token = GenerateJwt(adminUser, signInfo, configuration);
        client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse($"Bearer {token}");
        User employeeUser =
            new()
            {
                Email = "employee@gmail.com"
            };
        HttpResponseMessage userResponse = await client.PostAsJsonAsync(Routes.ApiV1 + Routes.Users, employeeUser, JsonSerializerOptions);
        Assert.Equal(HttpStatusCode.Created, userResponse.StatusCode);
        User createdEmployeeUser = (await userResponse.Content.ReadFromJsonAsync<User>(JsonSerializerOptions))!;
        Claim employeeClaim =
            new()
            {
                Type = ClaimTypes.Role,
                Value = ClaimTypes.Values.RoleEmployee
            };
        HttpResponseMessage claimResponse = await client.PostAsJsonAsync(Routes.ApiV1 + Routes.Users + $"/{createdEmployeeUser.Id}" + Routes.Claims, employeeClaim, JsonSerializerOptions);
        Assert.Equal(HttpStatusCode.Created, claimResponse.StatusCode);

        // create customer - as employee user
        token = GenerateJwt(employeeUser.Email, signInfo, configuration);
        client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse($"Bearer {token}");
        Customer newCustomer =
            new()
            {
                Name = "Another Name",
                Description = "This is another customer with another name"
            };
        HttpResponseMessage response = await client.PostAsJsonAsync(Routes.ApiV1 + Routes.Customers, newCustomer, JsonSerializerOptions);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        string body = await response.Content.ReadAsStringAsync();
        Assert.NotNull(body);
        Uri? location = response.Headers.Location;
        Assert.NotNull(location);

        response = await client.GetAsync(location);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        body = await response.Content.ReadAsStringAsync();
        Customer? customer = JsonSerializer.Deserialize<Customer>(body, JsonSerializerOptions);
        Assert.NotNull(customer);

        Assert.Equal(newCustomer.Name, customer!.Name);
        Assert.Equal(newCustomer.Description, customer!.Description);

        await TearDownDatabaseAsync(factory.Services);
    }

    [Fact]
    public async Task customer_can_be_updated()
    {
        // initialize keys
        var signInfo = GenerateRandomKeys(true);

        // setup api host
        await using WebApplicationFactory<Program> factory = GenerateWebApplicationFactory(UseConnectionString(), UseMockJwtToken(signInfo.ValidateKey));
        await InitializeDatabaseAsync(factory.Services);
        HttpClient client = factory.CreateClient();

        // create user and claims - as admin user
        IConfiguration configuration = factory.Services.GetRequiredService<IConfiguration>();
        string adminUser = configuration["Authentication:AdminAccount"];
        string token = GenerateJwt(adminUser, signInfo, configuration);
        client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse($"Bearer {token}");
        User employeeUser =
            new()
            {
                Email = "employee@gmail.com"
            };
        HttpResponseMessage userResponse = await client.PostAsJsonAsync(Routes.ApiV1 + Routes.Users, employeeUser, JsonSerializerOptions);
        Assert.Equal(HttpStatusCode.Created, userResponse.StatusCode);
        User createdEmployeeUser = (await userResponse.Content.ReadFromJsonAsync<User>(JsonSerializerOptions))!;
        Claim employeeClaim =
            new()
            {
                Type = ClaimTypes.Role,
                Value = ClaimTypes.Values.RoleEmployee
            };
        HttpResponseMessage claimResponse = await client.PostAsJsonAsync(Routes.ApiV1 + Routes.Users + $"/{createdEmployeeUser.Id}" + Routes.Claims, employeeClaim, JsonSerializerOptions);
        Assert.Equal(HttpStatusCode.Created, claimResponse.StatusCode);

        // create customer - as employee user
        token = GenerateJwt(employeeUser.Email, signInfo, configuration);
        client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse($"Bearer {token}");
        Customer newCustomer =
            new()
            {
                Name = "Initial name",
                Description = "Initial description"
            };
        HttpResponseMessage response = await client.PostAsJsonAsync(Routes.ApiV1 + Routes.Customers, newCustomer, JsonSerializerOptions);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Uri? location = response.Headers.Location;
        Assert.NotNull(location);

        response = await client.GetAsync(location);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Customer? customer = await response.Content.ReadFromJsonAsync<Customer>(JsonSerializerOptions);
        Assert.NotNull(customer);

        newCustomer.Name = "New name";
        newCustomer.Description = "New description";
        response = await client.PutAsJsonAsync(location, newCustomer, JsonSerializerOptions);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Customer? updatedCustomer = await response.Content.ReadFromJsonAsync<Customer>(JsonSerializerOptions);
        Assert.NotNull(updatedCustomer);
        Assert.Equal(newCustomer.Name, updatedCustomer!.Name);
        Assert.Equal(newCustomer.Description, updatedCustomer.Description);

        response = await client.GetAsync(location);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        updatedCustomer = await response.Content.ReadFromJsonAsync<Customer>(JsonSerializerOptions);
        Assert.NotNull(updatedCustomer);
        Assert.Equal(newCustomer.Name, updatedCustomer!.Name);
        Assert.Equal(newCustomer.Description, updatedCustomer.Description);

        await TearDownDatabaseAsync(factory.Services);
    }

    [Fact]
    public async Task customer_can_be_deleted()
    {
        // initialize keys
        var signInfo = GenerateRandomKeys(true);

        // setup api host
        await using WebApplicationFactory<Program> factory = GenerateWebApplicationFactory(UseConnectionString(), UseMockJwtToken(signInfo.ValidateKey));
        await InitializeDatabaseAsync(factory.Services);
        HttpClient client = factory.CreateClient();

        // create user and claims - as admin user
        IConfiguration configuration = factory.Services.GetRequiredService<IConfiguration>();
        string adminUser = configuration["Authentication:AdminAccount"];
        string token = GenerateJwt(adminUser, signInfo, configuration);
        client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse($"Bearer {token}");
        User employeeUser =
            new()
            {
                Email = "employee@gmail.com"
            };
        HttpResponseMessage userResponse = await client.PostAsJsonAsync(Routes.ApiV1 + Routes.Users, employeeUser, JsonSerializerOptions);
        Assert.Equal(HttpStatusCode.Created, userResponse.StatusCode);
        User createdEmployeeUser = (await userResponse.Content.ReadFromJsonAsync<User>(JsonSerializerOptions))!;
        Claim employeeClaim =
            new()
            {
                Type = ClaimTypes.Role,
                Value = ClaimTypes.Values.RoleEmployee
            };
        HttpResponseMessage claimResponse = await client.PostAsJsonAsync(Routes.ApiV1 + Routes.Users + $"/{createdEmployeeUser.Id}" + Routes.Claims, employeeClaim, JsonSerializerOptions);
        Assert.Equal(HttpStatusCode.Created, claimResponse.StatusCode);

        // create customer - as employee user
        token = GenerateJwt(employeeUser.Email, signInfo, configuration);
        client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse($"Bearer {token}");
        Customer newCustomer =
            new()
            {
                Name = "Another Name",
                Description = "This is another customer with another name"
            };
        HttpResponseMessage response = await client.PostAsJsonAsync(Routes.ApiV1 + Routes.Customers, newCustomer, JsonSerializerOptions);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Uri? location = response.Headers.Location;
        Assert.NotNull(location);

        response = await client.GetAsync(location);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Customer? customer = await response.Content.ReadFromJsonAsync<Customer>(JsonSerializerOptions);
        Assert.NotNull(customer);

        response = await client.DeleteAsync(location);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        response = await client.GetAsync(location);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        await TearDownDatabaseAsync(factory.Services);
    }
}

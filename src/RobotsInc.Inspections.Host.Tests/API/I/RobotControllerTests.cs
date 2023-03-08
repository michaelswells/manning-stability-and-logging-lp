using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using RobotsInc.Inspections.API.I;
using RobotsInc.Inspections.API.I.Security;
using RobotsInc.Inspections.BusinessLogic;

using Xunit;

using ArticulatedRobot = RobotsInc.Inspections.API.I.ArticulatedRobot;
using Customer = RobotsInc.Inspections.Models.Customer;

namespace RobotsInc.Inspections.Host.API.I;

public class RobotControllerTests : ControllerTests
{
    public async Task<long> CreateCustomerInDatabase(IServiceProvider serviceProvider)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        ICustomerManager customerManager = scope.ServiceProvider.GetRequiredService<ICustomerManager>();
        Customer model =
            new()
            {
                Name = "Customer"
            };
        await customerManager.SaveAsync(model, CancellationToken.None);
        Assert.NotNull(model.Id);
        return model.Id.GetValueOrDefault();
    }

    [Fact]
    public async Task articulated_robot_with_missing_joints_gets_bad_request()
    {
        // initialize keys
        var signInfo = GenerateRandomKeys(true);

        // setup api host
        await using WebApplicationFactory<Program> factory = GenerateWebApplicationFactory(UseConnectionString(), UseMockJwtToken(signInfo.ValidateKey));
        await InitializeDatabaseAsync(factory.Services);
        long customerId = await CreateCustomerInDatabase(factory.Services);
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

        ArticulatedRobot newArticulatedRobot =
            new()
            {
                ManufacturingDate = new DateTime(2015, 9, 18),
                SerialNumber = "1234567890123456"
            };
        HttpResponseMessage response = await client.PostAsJsonAsync(Routes.ApiV1 + Routes.Customers + $"/{customerId}" + Routes.Robots, newArticulatedRobot, JsonSerializerOptions);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        string body = await response.Content.ReadAsStringAsync();
        Assert.NotNull(body);
        ValidationProblemDetails? details = JsonSerializer.Deserialize<ValidationProblemDetails>(body, JsonSerializerOptions);
        Assert.NotNull(details);
        Assert.Contains(nameof(ArticulatedRobot.NrOfJoints), details!.Errors.Keys);
        await TearDownDatabaseAsync(factory.Services);
    }

    [Fact]
    public async Task articulated_robot_with_complete_info_can_be_created_and_fetched()
    {
        // initialize keys
        var signInfo = GenerateRandomKeys(true);

        // setup api host
        await using WebApplicationFactory<Program> factory = GenerateWebApplicationFactory(UseConnectionString(), UseMockJwtToken(signInfo.ValidateKey));
        await InitializeDatabaseAsync(factory.Services);
        long customerId = await CreateCustomerInDatabase(factory.Services);
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

        ArticulatedRobot newArticulatedRobot =
            new()
            {
                ManufacturingDate = new DateTime(2015, 9, 18),
                SerialNumber = "1234567890123456",
                NrOfJoints = 234
            };
        HttpResponseMessage response = await client.PostAsJsonAsync(Routes.ApiV1 + Routes.Customers + $"/{customerId}" + Routes.Robots, newArticulatedRobot, JsonSerializerOptions);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Uri? location = response.Headers.Location;
        Assert.NotNull(location);
        Robot? robot = await response.Content.ReadFromJsonAsync<Robot>(JsonSerializerOptions);
        Assert.NotNull(robot);
        Assert.IsType<ArticulatedRobot>(robot);
        ArticulatedRobot articulatedRobot = (robot as ArticulatedRobot)!;
        Assert.Equal(newArticulatedRobot.SerialNumber, articulatedRobot.SerialNumber);
        Assert.Equal(newArticulatedRobot.ManufacturingDate, articulatedRobot.ManufacturingDate);
        Assert.Equal(newArticulatedRobot.NrOfJoints, articulatedRobot.NrOfJoints);

        response = await client.GetAsync(location);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        ArticulatedRobot? fetchedArticulatedRobot = await response.Content.ReadFromJsonAsync<ArticulatedRobot>(JsonSerializerOptions);
        Assert.NotNull(fetchedArticulatedRobot);
        Assert.Equal(articulatedRobot.Id, fetchedArticulatedRobot?.Id);

        await TearDownDatabaseAsync(factory.Services);
    }

    [Fact]
    public async Task articulated_robot_can_be_updated()
    {
        // initialize keys
        var signInfo = GenerateRandomKeys(true);

        // setup api host
        await using WebApplicationFactory<Program> factory = GenerateWebApplicationFactory(UseConnectionString(), UseMockJwtToken(signInfo.ValidateKey));
        await InitializeDatabaseAsync(factory.Services);
        long customerId = await CreateCustomerInDatabase(factory.Services);
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

        ArticulatedRobot newArticulatedRobot =
            new()
            {
                ManufacturingDate = new DateTime(2015, 9, 18),
                SerialNumber = "1234567890123456",
                NrOfJoints = 234
            };
        HttpResponseMessage response = await client.PostAsJsonAsync(Routes.ApiV1 + Routes.Customers + $"/{customerId}" + Routes.Robots, newArticulatedRobot, JsonSerializerOptions);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Uri? location = response.Headers.Location;
        Assert.NotNull(location);

        newArticulatedRobot.NrOfJoints = 12;
        newArticulatedRobot.Description = "Robot for grabbing stuff";

        response = await client.PutAsJsonAsync(location, newArticulatedRobot, JsonSerializerOptions);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        ArticulatedRobot? articulatedRobot = await response.Content.ReadFromJsonAsync<ArticulatedRobot>(JsonSerializerOptions);
        Assert.NotNull(articulatedRobot);
        Assert.Equal(newArticulatedRobot.SerialNumber, articulatedRobot!.SerialNumber);
        Assert.Equal(newArticulatedRobot.ManufacturingDate, articulatedRobot.ManufacturingDate);
        Assert.Equal(newArticulatedRobot.NrOfJoints, articulatedRobot.NrOfJoints);
        Assert.Equal(newArticulatedRobot.Description, articulatedRobot.Description);

        response = await client.GetAsync(location);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        ArticulatedRobot? fetchedArticulatedRobot = await response.Content.ReadFromJsonAsync<ArticulatedRobot>(JsonSerializerOptions);
        Assert.NotNull(fetchedArticulatedRobot);
        Assert.Equal(articulatedRobot.Id, fetchedArticulatedRobot?.Id);

        await TearDownDatabaseAsync(factory.Services);
    }

    [Fact]
    public async Task articulated_robot_can_be_deleted()
    {
        // initialize keys
        var signInfo = GenerateRandomKeys(true);

        // setup api host
        await using WebApplicationFactory<Program> factory = GenerateWebApplicationFactory(UseConnectionString(), UseMockJwtToken(signInfo.ValidateKey));
        await InitializeDatabaseAsync(factory.Services);
        long customerId = await CreateCustomerInDatabase(factory.Services);
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

        ArticulatedRobot newArticulatedRobot =
            new()
            {
                ManufacturingDate = new DateTime(2015, 9, 18),
                SerialNumber = "1234567890123456",
                NrOfJoints = 234
            };
        HttpResponseMessage response = await client.PostAsJsonAsync(Routes.ApiV1 + Routes.Customers + $"/{customerId}" + Routes.Robots, newArticulatedRobot, JsonSerializerOptions);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Uri? location = response.Headers.Location;
        Assert.NotNull(location);

        response = await client.DeleteAsync(location);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        response = await client.GetAsync(location);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        await TearDownDatabaseAsync(factory.Services);
    }

    [Fact]
    public async Task automated_guided_vehicle_with_missing_charging_type_gets_bad_request()
    {
        // initialize keys
        var signInfo = GenerateRandomKeys(true);

        // setup api host
        await using WebApplicationFactory<Program> factory = GenerateWebApplicationFactory(UseConnectionString(), UseMockJwtToken(signInfo.ValidateKey));
        await InitializeDatabaseAsync(factory.Services);
        long customerId = await CreateCustomerInDatabase(factory.Services);
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

        AutomatedGuidedVehicle newAutomatedGuidedVehicle =
            new()
            {
                ManufacturingDate = new DateTime(2015, 9, 18),
                SerialNumber = "1234567890123456",
                NavigationType = NavigationType.LASER
            };
        HttpResponseMessage response = await client.PostAsJsonAsync(Routes.ApiV1 + Routes.Customers + $"/{customerId}" + Routes.Robots, newAutomatedGuidedVehicle, JsonSerializerOptions);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        string body = await response.Content.ReadAsStringAsync();
        Assert.NotNull(body);
        ValidationProblemDetails? details = JsonSerializer.Deserialize<ValidationProblemDetails>(body, JsonSerializerOptions);
        Assert.NotNull(details);
        Assert.Contains(nameof(AutomatedGuidedVehicle.ChargingType), details!.Errors.Keys);
        await TearDownDatabaseAsync(factory.Services);
    }

    [Fact]
    public async Task automated_guided_vehicle_with_complete_info_can_be_created_and_fetched()
    {
        // initialize keys
        var signInfo = GenerateRandomKeys(true);

        // setup api host
        await using WebApplicationFactory<Program> factory = GenerateWebApplicationFactory(UseConnectionString(), UseMockJwtToken(signInfo.ValidateKey));
        await InitializeDatabaseAsync(factory.Services);
        long customerId = await CreateCustomerInDatabase(factory.Services);
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

        AutomatedGuidedVehicle newAutomatedGuidedVehicle =
            new()
            {
                ManufacturingDate = new DateTime(2015, 9, 18),
                SerialNumber = "1234567890123456",
                ChargingType = ChargingType.AUTOMATIC_BATTERY_SWAP,
                NavigationType = NavigationType.LASER
            };
        HttpResponseMessage response = await client.PostAsJsonAsync(Routes.ApiV1 + Routes.Customers + $"/{customerId}" + Routes.Robots, newAutomatedGuidedVehicle, JsonSerializerOptions);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Uri? location = response.Headers.Location;
        Assert.NotNull(location);
        Robot? robot = await response.Content.ReadFromJsonAsync<Robot>(JsonSerializerOptions);
        Assert.NotNull(robot);
        Assert.IsType<AutomatedGuidedVehicle>(robot);
        AutomatedGuidedVehicle automatedGuidedVehicle = (robot as AutomatedGuidedVehicle)!;
        Assert.Equal(newAutomatedGuidedVehicle.SerialNumber, automatedGuidedVehicle.SerialNumber);
        Assert.Equal(newAutomatedGuidedVehicle.ManufacturingDate, automatedGuidedVehicle.ManufacturingDate);
        Assert.Equal(newAutomatedGuidedVehicle.ChargingType, automatedGuidedVehicle.ChargingType);
        Assert.Equal(newAutomatedGuidedVehicle.NavigationType, automatedGuidedVehicle.NavigationType);

        response = await client.GetAsync(location);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AutomatedGuidedVehicle? fetchedAutomatedGuidedVehicle = await response.Content.ReadFromJsonAsync<AutomatedGuidedVehicle>(JsonSerializerOptions);
        Assert.NotNull(fetchedAutomatedGuidedVehicle);
        Assert.Equal(automatedGuidedVehicle.Id, fetchedAutomatedGuidedVehicle?.Id);

        await TearDownDatabaseAsync(factory.Services);
    }

    [Fact]
    public async Task automated_guided_vehicle_can_be_updated()
    {
        // initialize keys
        var signInfo = GenerateRandomKeys(true);

        // setup api host
        await using WebApplicationFactory<Program> factory = GenerateWebApplicationFactory(UseConnectionString(), UseMockJwtToken(signInfo.ValidateKey));
        await InitializeDatabaseAsync(factory.Services);
        long customerId = await CreateCustomerInDatabase(factory.Services);
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

        AutomatedGuidedVehicle newAutomatedGuidedVehicle =
            new()
            {
                ManufacturingDate = new DateTime(2015, 9, 18),
                SerialNumber = "1234567890123456",
                NavigationType = NavigationType.GUIDE_TAPE,
                ChargingType = ChargingType.MANUAL_BATTERY_SWAP
            };
        HttpResponseMessage response = await client.PostAsJsonAsync(Routes.ApiV1 + Routes.Customers + $"/{customerId}" + Routes.Robots, newAutomatedGuidedVehicle, JsonSerializerOptions);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Uri? location = response.Headers.Location;
        Assert.NotNull(location);

        newAutomatedGuidedVehicle.NavigationType = NavigationType.LASER;
        newAutomatedGuidedVehicle.ChargingType = ChargingType.AUTOMATIC_CHARGING;
        newAutomatedGuidedVehicle.Description = "Robot for grabbing stuff";

        response = await client.PutAsJsonAsync(location, newAutomatedGuidedVehicle, JsonSerializerOptions);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AutomatedGuidedVehicle? automatedGuidedVehicle = await response.Content.ReadFromJsonAsync<AutomatedGuidedVehicle>(JsonSerializerOptions);
        Assert.NotNull(automatedGuidedVehicle);
        Assert.Equal(newAutomatedGuidedVehicle.SerialNumber, automatedGuidedVehicle!.SerialNumber);
        Assert.Equal(newAutomatedGuidedVehicle.ManufacturingDate, automatedGuidedVehicle.ManufacturingDate);
        Assert.Equal(newAutomatedGuidedVehicle.ChargingType, automatedGuidedVehicle.ChargingType);
        Assert.Equal(newAutomatedGuidedVehicle.NavigationType, automatedGuidedVehicle.NavigationType);
        Assert.Equal(newAutomatedGuidedVehicle.Description, automatedGuidedVehicle.Description);

        response = await client.GetAsync(location);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AutomatedGuidedVehicle? fetchedAutomatedGuidedVehicle = await response.Content.ReadFromJsonAsync<AutomatedGuidedVehicle>(JsonSerializerOptions);
        Assert.NotNull(fetchedAutomatedGuidedVehicle);
        Assert.Equal(automatedGuidedVehicle.Id, fetchedAutomatedGuidedVehicle?.Id);

        await TearDownDatabaseAsync(factory.Services);
    }

    [Fact]
    public async Task automated_guided_vehicle_can_be_deleted()
    {
        // initialize keys
        var signInfo = GenerateRandomKeys(true);

        // setup api host
        await using WebApplicationFactory<Program> factory = GenerateWebApplicationFactory(UseConnectionString(), UseMockJwtToken(signInfo.ValidateKey));
        await InitializeDatabaseAsync(factory.Services);
        long customerId = await CreateCustomerInDatabase(factory.Services);
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

        AutomatedGuidedVehicle newAutomatedGuidedVehicle =
            new()
            {
                ManufacturingDate = new DateTime(2015, 9, 18),
                SerialNumber = "1234567890123456",
                ChargingType = ChargingType.AUTOMATIC_CHARGING,
                NavigationType = NavigationType.WIRED
            };
        HttpResponseMessage response = await client.PostAsJsonAsync(Routes.ApiV1 + Routes.Customers + $"/{customerId}" + Routes.Robots, newAutomatedGuidedVehicle, JsonSerializerOptions);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Uri? location = response.Headers.Location;
        Assert.NotNull(location);

        response = await client.DeleteAsync(location);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        response = await client.GetAsync(location);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        await TearDownDatabaseAsync(factory.Services);
    }
}

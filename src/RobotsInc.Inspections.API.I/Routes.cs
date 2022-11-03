namespace RobotsInc.Inspections.API.I;

public static class Routes
{
    // api version
    public const string ApiVersion = "/api/v{version:apiVersion}";
    public const string ApiV1 = "/api/v1";
    public const string ApiV2 = "/api/v2";

    // paths
    public const string Health = "/health";

    public const string Customers = "/customers";
    public const string Robots = "/robots";
    public const string Inspections = "/inspections";
    public const string Notes = "/notes";
    public const string Photos = "/photos";

    public const string Users = "/users";
    public const string Claims = "/claims";

    public const string Search = "/search";
}

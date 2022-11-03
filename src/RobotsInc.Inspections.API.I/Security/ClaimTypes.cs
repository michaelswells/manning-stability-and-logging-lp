namespace RobotsInc.Inspections.API.I.Security;

public static class ClaimTypes
{
    /// <summary>
    ///     The claim type used for the name.
    /// </summary>
    /// <remarks>
    ///     This should not be used within a custom claim in Inspections, but can be used to read existing claims.
    ///     The name claim will always be added by the external identity provider.
    /// </remarks>
    public static readonly string Email = "email";

    /// <summary>
    ///     The claim type to use for roles.
    /// </summary>
    public static readonly string Role = "role";

    /// <summary>
    ///     The claim type to use for the link to a customer.
    /// </summary>
    public static readonly string Customer = "customer";

    /// <summary>
    ///     The claim type to indicate that claims were enriched.
    /// </summary>
    public static readonly string Enriched = "enriched";

    public static class Values
    {
        /// <summary>
        ///     The 'employee' role is used for an internal RobotsInc user.
        /// </summary>
        public static readonly string RoleEmployee = "employee";

        /// <summary>
        ///     The 'customer' role is used for an external user that is linked to a customer of RobotsInc.
        /// </summary>
        public static readonly string RoleCustomer = "customer";
    }
}

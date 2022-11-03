using System.Diagnostics.CodeAnalysis;

namespace RobotsInc.Inspections.Server.API;

[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Justified")]
[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:Field names should not contain underscore", Justification = "Justified")]
public static class Routes
{
    public const string Customer_GetById = nameof(Customer_GetById);
    public const string Robot_GetById = nameof(Robot_GetById);
    public const string Inspection_GetById = nameof(Inspection_GetById);
    public const string Note_GetById = nameof(Note_GetById);
    public const string Photo_GetById = nameof(Photo_GetById);
    public const string User_GetById = nameof(User_GetById);
    public const string Claim_GetById = nameof(Claim_GetById);
}

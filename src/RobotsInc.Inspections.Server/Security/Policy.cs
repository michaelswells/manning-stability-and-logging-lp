namespace RobotsInc.Inspections.Server.Security;

public enum Policy
{
    /// <summary>
    ///     Policy for creating, updating and deleting inspections.
    /// </summary>
    EDIT_INSPECTIONS = 1,

    /// <summary>
    ///     Policy for consulting inspections in read-only mode.
    /// </summary>
    CONSULT_INSPECTIONS,

    /// <summary>
    ///     Policy for managing the user claims.
    /// </summary>
    MANAGE_USER_CLAIMS
}

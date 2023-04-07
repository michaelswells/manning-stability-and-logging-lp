namespace RobotsInc.Inspections.Server.Filters;

public class InvalidPropertyException : System.Exception
{
    public string PropertyName { get; }

    public InvalidPropertyException(string propertyName, string message)
        : base(message)
    {
        PropertyName = propertyName;
    }
}

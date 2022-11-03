using System.ComponentModel.DataAnnotations;

using Swashbuckle.AspNetCore.Annotations;

namespace RobotsInc.Inspections.API.I.Security;

public class User
{
    [SwaggerSchema(ReadOnly = true)]
    public long? Id { get; set; }

    [Required(AllowEmptyStrings = false)]
    [StringLength(100)]
    public string? Email { get; set; }
}

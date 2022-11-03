using System.ComponentModel.DataAnnotations;

using Swashbuckle.AspNetCore.Annotations;

namespace RobotsInc.Inspections.API.I.Security;

public class Claim
{
    [SwaggerSchema(ReadOnly = true)]
    public long? Id { get; set; }

    [Required(AllowEmptyStrings = false)]
    [StringLength(256)]
    public string? Type { get; set; }

    [Required(AllowEmptyStrings = false)]
    [StringLength(256)]
    public string? Value { get; set; }
}

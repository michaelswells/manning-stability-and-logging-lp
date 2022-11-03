using System.ComponentModel.DataAnnotations;

using Swashbuckle.AspNetCore.Annotations;

namespace RobotsInc.Inspections.API.I;

public class Customer
{
    [SwaggerSchema(ReadOnly = true)]
    public long? Id { get; set; }

    [Required(AllowEmptyStrings = false)]
    [StringLength(30)]
    public string? Name { get; set; }

    [StringLength(250)]
    public string? Description { get; set; }
}

using System.ComponentModel.DataAnnotations;

using Swashbuckle.AspNetCore.Annotations;

namespace RobotsInc.Inspections.API.I;

public class Note
{
    [SwaggerSchema(ReadOnly = true)]
    public long? Id { get; set; }

    [Required]
    [StringLength(50)]
    public string? Summary { get;  set; }

    [Required]
    public ImportanceLevel? Importance { get; set; }

    [StringLength(512)]
    public string? Description { get; set; }
}

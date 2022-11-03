using System;
using System.ComponentModel.DataAnnotations;

using Swashbuckle.AspNetCore.Annotations;

namespace RobotsInc.Inspections.API.I;

public class Inspection
{
    [SwaggerSchema(ReadOnly = true)]
    public long? Id { get; set; }

    [Required]
    public DateTime? Date { get; set; }

    [Required]
    public InspectionState? State { get; set; }

    [StringLength(512)]
    public string? Summary { get; set; }
}

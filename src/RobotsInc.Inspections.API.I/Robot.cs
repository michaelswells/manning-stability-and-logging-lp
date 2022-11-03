using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using Swashbuckle.AspNetCore.Annotations;

namespace RobotsInc.Inspections.API.I;

[SwaggerDiscriminator("robotType")]
[SwaggerSubType(typeof(ArticulatedRobot), DiscriminatorValue = nameof(I.RobotType.ARTICULATED_ROBOT))]
[SwaggerSubType(typeof(AutomatedGuidedVehicle), DiscriminatorValue = nameof(I.RobotType.AUTOMATED_GUIDED_VEHICLE))]
public abstract class Robot
{
    [SwaggerSchema(ReadOnly = true)]
    public long? Id { get; set; }

    [Required(AllowEmptyStrings = false)]
    [StringLength(16, MinimumLength = 16)]
    public string? SerialNumber { get; set; }

    [Required]
    public DateTime? ManufacturingDate { get; set; }

    [StringLength(512)]
    public string? Description { get; set; }

    [JsonPropertyOrder(int.MinValue)]
    public abstract RobotType RobotType { get; }
}

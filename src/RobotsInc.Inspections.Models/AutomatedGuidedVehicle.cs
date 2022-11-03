using System.ComponentModel.DataAnnotations;

using RobotsInc.Inspections.API.I;

namespace RobotsInc.Inspections.Models;

public class AutomatedGuidedVehicle : Robot
{
    [Required]
    public virtual NavigationType? NavigationType { get; set; }

    [Required]
    public virtual ChargingType? ChargingType { get; set; }

    public override RobotType RobotType
        => RobotType.AUTOMATED_GUIDED_VEHICLE;
}

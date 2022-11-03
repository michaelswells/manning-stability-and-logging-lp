using System.ComponentModel.DataAnnotations;

namespace RobotsInc.Inspections.API.I;

public class AutomatedGuidedVehicle : Robot
{
    [Required]
    public NavigationType? NavigationType { get; set; }

    [Required]
    public ChargingType? ChargingType { get; set; }

    public override RobotType RobotType
        => RobotType.AUTOMATED_GUIDED_VEHICLE;
}

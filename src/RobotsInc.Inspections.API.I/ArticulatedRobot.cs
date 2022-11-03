using System.ComponentModel.DataAnnotations;

namespace RobotsInc.Inspections.API.I;

public class ArticulatedRobot : Robot
{
    [Required]
    public int? NrOfJoints { get; set; }

    public override RobotType RobotType
        => RobotType.ARTICULATED_ROBOT;
}

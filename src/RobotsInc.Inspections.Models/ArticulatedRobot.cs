using System.ComponentModel.DataAnnotations;

using RobotsInc.Inspections.API.I;

namespace RobotsInc.Inspections.Models;

public class ArticulatedRobot : Robot
{
    [Required]
    public virtual int? NrOfJoints { get; set; }

    public override RobotType RobotType
        => RobotType.ARTICULATED_ROBOT;
}

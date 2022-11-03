using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RobotsInc.Inspections.Models;

public class Customer
{
    private readonly ISet<Robot> _robots = new HashSet<Robot>();

    [Required]
    public virtual long? Id { get; set; }

    [Required(AllowEmptyStrings = false)]
    [StringLength(30)]
    public virtual string? Name { get; set; }

    [StringLength(250)]
    public virtual string? Description { get; set; }

    public virtual ISet<Robot> Robots
        => _robots;

    public virtual void AddRobot(Robot? robot)
    {
        if ((robot != null) && _robots.Add(robot))
        {
            robot.Customer = this;
        }
    }

    public virtual void RemoveRobot(Robot? robot)
    {
        if ((robot != null) && _robots.Remove(robot))
        {
            robot.Customer = null;
        }
    }
}

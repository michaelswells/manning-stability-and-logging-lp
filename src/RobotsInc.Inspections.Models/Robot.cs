using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using RobotsInc.Inspections.API.I;

namespace RobotsInc.Inspections.Models;

public abstract class Robot
{
    private readonly ISet<Inspection> _inspections = new HashSet<Inspection>();

    private Customer? _customer;

    [Required]
    public virtual long? Id { get; set; }

    [Required(AllowEmptyStrings = false)]
    [StringLength(16, MinimumLength = 16)]
    public virtual string? SerialNumber { get; set; }

    [Required]
    public virtual DateTime? ManufacturingDate { get; set; }

    [StringLength(512)]
    public virtual string? Description { get; set; }

    public abstract RobotType RobotType { get; }

    [Required]
    public virtual Customer? Customer
    {
        get => _customer;
        set
        {
            if (_customer != value)
            {
                if (_customer != null)
                {
                    Customer previousCustomer = _customer;
                    _customer = null;
                    previousCustomer.RemoveRobot(this);
                }

                _customer = value;
                _customer?.AddRobot(this);
            }
        }
    }

    public virtual ISet<Inspection> Inspections
        => _inspections;

    public virtual void AddInspection(Inspection? inspection)
    {
        if ((inspection != null) && _inspections.Add(inspection))
        {
            inspection.Robot = this;
        }
    }

    public virtual void RemoveInspection(Inspection? inspection)
    {
        if ((inspection != null) && _inspections.Remove(inspection))
        {
            inspection.Robot = null;
        }
    }
}

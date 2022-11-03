using System;
using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace RobotsInc.Inspections.API.I;

public class RobotSearchCriteria
{
    [Required]
    [Range(0, int.MaxValue)]
    public int? Page { get; set; }

    [Required]
    [Range(1, 1000)]
    public int? PageSize { get; set; }

    public long? CustomerId { get; set; }

    public RobotType? RobotType { get; set; }

    public DateTime? ManufacturingDateFrom { get; set; }

    public DateTime? ManufacturingDateTo { get; set; }

    [BindNever]
    public long[]? CustomerIds { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace RobotsInc.Inspections.API.I;

public class PagedList<TItem>
    where TItem : class
{
    [Required]
    [Range(0, int.MaxValue)]
    public int? Page { get; set; }

    [Required]
    [Range(1, 1000)]
    public int? PageSize { get; set; }

    [Required]
    public int? TotalCount { get; set; }

    [Required]
    public int? TotalPages { get; set; }

    [Required]
    public TItem[]? Items { get; set; }
}

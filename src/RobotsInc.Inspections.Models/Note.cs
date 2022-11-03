using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using RobotsInc.Inspections.API.I;

namespace RobotsInc.Inspections.Models;

public class Note
{
    private ISet<Photo> _photos = new HashSet<Photo>();

    private Inspection? _inspection;

    [Required]
    public virtual long? Id { get; set; }

    [Required]
    [StringLength(50)]
    public virtual string? Summary { get; set; }

    [Required]
    public virtual ImportanceLevel? Importance { get; set; }

    [StringLength(512)]
    public virtual string? Description { get; set; }

    [Required]
    public virtual Inspection? Inspection
    {
        get => _inspection;
        set
        {
            if (_inspection != value)
            {
                if (_inspection != null)
                {
                    Inspection previousInspection = _inspection;
                    _inspection = null;
                    previousInspection.RemoveNote(this);
                }

                _inspection = value;
                _inspection?.AddNote(this);
            }
        }
    }

    public virtual ISet<Photo> Photos
        => _photos;

    public virtual void AddNote(Photo? photo)
    {
        if ((photo != null) && _photos.Add(photo))
        {
            photo.Note = this;
        }
    }

    public virtual void RemoveNote(Photo? photo)
    {
        if ((photo != null) && _photos.Remove(photo))
        {
            photo.Note = null;
        }
    }
}

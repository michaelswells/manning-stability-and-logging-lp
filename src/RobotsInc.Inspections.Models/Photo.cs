using System.ComponentModel.DataAnnotations;

namespace RobotsInc.Inspections.Models;

public class Photo
{
    private Note? _note;

    [Required]
    public virtual long? Id { get; set; }

    [Required]
    public virtual byte[]? Content { get; set; }

    [Required]
    public virtual Note? Note
    {
        get => _note;
        set
        {
            if (_note != value)
            {
                if (_note != null)
                {
                    Note previousNote = _note;
                    _note = null;
                    previousNote.RemoveNote(this);
                }

                _note = value;
                _note?.AddNote(this);
            }
        }
    }
}

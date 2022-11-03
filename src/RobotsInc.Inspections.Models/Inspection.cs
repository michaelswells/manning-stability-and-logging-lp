using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using RobotsInc.Inspections.API.I;

namespace RobotsInc.Inspections.Models;

public class Inspection
{
    private readonly ISet<Note> _notes = new HashSet<Note>();

    private Robot? _robot;

    [Required]
    public virtual long? Id { get; set; }

    [Required]
    public virtual DateTime? Date { get; set; }

    [Required]
    public virtual InspectionState? State { get; set; }

    [StringLength(512)]
    public virtual string? Summary { get; set; }

    [Required]
    public virtual Robot? Robot
    {
        get => _robot;
        set
        {
            if (_robot != value)
            {
                if (_robot != null)
                {
                    Robot previousRobot = _robot;
                    _robot = null;
                    previousRobot.RemoveInspection(this);
                }

                _robot = value;
                _robot?.AddInspection(this);
            }
        }
    }

    public virtual ISet<Note> Notes
        => _notes;

    public virtual void AddNote(Note? note)
    {
        if ((note != null) && _notes.Add(note))
        {
            note.Inspection = this;
        }
    }

    public virtual void RemoveNote(Note? note)
    {
        if ((note != null) && _notes.Remove(note))
        {
            note.Inspection = null;
        }
    }
}

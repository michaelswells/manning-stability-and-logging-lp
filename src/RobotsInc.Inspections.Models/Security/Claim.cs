using System.ComponentModel.DataAnnotations;

namespace RobotsInc.Inspections.Models.Security;

public class Claim
{
    private User? _user;

    [Required]
    public virtual long? Id { get; set; }

    [Required(AllowEmptyStrings = false)]
    [StringLength(256)]
    public virtual string? Type { get; set; }

    [Required(AllowEmptyStrings = false)]
    [StringLength(256)]
    public virtual string? Value { get; set; }

    [Required]
    public virtual User? User
    {
        get => _user;
        set
        {
            if (_user != value)
            {
                if (_user != null)
                {
                    User previousUser = _user;
                    _user = null;
                    previousUser.RemoveClaim(this);
                }

                _user = value;
                _user?.AddClaim(this);
            }
        }
    }
}

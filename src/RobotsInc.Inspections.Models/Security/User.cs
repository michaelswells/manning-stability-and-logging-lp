using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RobotsInc.Inspections.Models.Security;

public class User
{
    private readonly ISet<Claim> _claims = new HashSet<Claim>();

    [Required]
    public virtual long? Id { get; set; }

    [Required(AllowEmptyStrings = false)]
    [StringLength(100)]
    public virtual string? Email { get; set; }

    public virtual ISet<Claim> Claims
        => _claims;

    public virtual void AddClaim(Claim? claim)
    {
        if ((claim != null) && _claims.Add(claim))
        {
            claim.User = this;
        }
    }

    public virtual void RemoveClaim(Claim? claim)
    {
        if ((claim != null) && _claims.Remove(claim))
        {
            claim.User = null;
        }
    }
}

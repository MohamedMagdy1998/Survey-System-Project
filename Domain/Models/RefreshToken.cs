using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models;

[Owned]
public class RefreshToken
{
    public string Token { get; set; } = string.Empty;

    public DateTime Expiration { get; set; } 

    public DateTime CreatedOn { get; set;} = DateTime.UtcNow;

    public DateTime? RevokedOn { get; set; }

    public bool IsExpired => DateTime.UtcNow >= Expiration;
    public bool IsActive => RevokedOn is null && !IsExpired;

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models;

public class AuditableEntity
{
    public string CreatedById { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }

    public string? UpdatedById { get; set; }
    public DateTime UpdatedOn { get; set; }

    public ApplicationUser CreatedBy { get; set; } = default!;

    public ApplicationUser? UpdatedBy { get;set; } 


}

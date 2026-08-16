using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models;

public sealed class Answer : AuditableEntity
{
    public int Id { get; set; }

    public string Content { get; set; } = string.Empty;

    public int QuestionId { get; set; }

    public bool IsActive { get; set; } = true;


    public Question Question { get; set; } = default!;
}
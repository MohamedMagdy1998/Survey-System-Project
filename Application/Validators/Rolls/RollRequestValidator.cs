using Application.DTOs.Requests.Rolls;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators.Rolls;

public class RoleRequestValidator : AbstractValidator<RoleRequest>
{
    public RoleRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .Length(3, 200);

        RuleFor(x => x.Permissions)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.Permissions)
            .Must(x => x.Distinct().Count() == x.Count)
            .WithMessage("You cannot add duplicated permissions for the same role")
            .When(x => x.Permissions != null);
    }
}
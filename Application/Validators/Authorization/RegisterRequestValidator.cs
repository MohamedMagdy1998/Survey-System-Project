using Application.DTOs.Requests.Authorization;
using Application.Validators.Helpers;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators.Authorization;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {

        RuleFor(x => x.Email)
          .NotEmpty()
          .EmailAddress();

        RuleFor(x => x.FirstName).NotEmpty()
            .Length(3, 100).WithMessage("First Name length should be between 3 and 100");

        RuleFor(x => x.LastName).NotEmpty()
            .Length(3, 100).WithMessage("Last Name length should be between 3 and 100");

        RuleFor(x => x.Password).NotEmpty()
            .MinimumLength(8)
            .Matches(RegexPatterns.Password)
            .WithMessage("Password should be at least 8 digits and should contains Lowercase, NonAlphanumeric and Uppercase");







    }
}

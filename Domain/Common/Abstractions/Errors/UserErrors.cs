using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Common.Abstractions.Errors;

public static class UserErrors
{
    // HTTP 404
    public static readonly Error NotFound =
        Error.NotFound("Users.NotFound", "The requested user was not found.");

    // HTTP 400
    public static readonly Error InvalidCredentials =
        Error.BadRequest("Users.InvalidCredentials", "Invalid email or password.");

    // HTTP 409
    public static readonly Error DuplicateEmail =
        Error.Conflict("Users.DuplicateEmail", "A user with this email address already exists.");

    // HTTP 401
    public static readonly Error Unauthorized =
        Error.Unauthorized("Users.Unauthorized", "You must be logged in to perform this action.");

    // HTTP 403
    public static readonly Error Forbidden =
        Error.Forbidden("Users.Forbidden", "You do not have permission to access this resource.");

    // HTTP 400
    public static readonly Error AccountLocked =
        Error.BadRequest("Users.AccountLocked", "This account has been locked due to multiple failed login attempts.");

    // HTTP 400
    public static readonly Error EmailNotConfirmed =
        Error.BadRequest("Users.EmailNotConfirmed", "Email address has not been confirmed yet.");
}

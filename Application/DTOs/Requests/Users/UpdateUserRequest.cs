using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Requests.Users;

public record UpdateUserRequest(
    string FirstName,
    string LastName,
    string Email,
    IList<string> Roles
);
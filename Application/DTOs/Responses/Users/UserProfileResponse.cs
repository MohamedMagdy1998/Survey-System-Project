using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Responses.Users;

public record UserProfileResponse(
    string Email,
    string UserName,
    string FirstName,
    string LastName
);
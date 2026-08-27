using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Requests.Users;

public record UpdateProfileRequest(
    string FirstName,
    string LastName
);

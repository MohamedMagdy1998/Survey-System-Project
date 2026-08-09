using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Responses.Authorization;

public record AuthResponse
    (
    string Id,
    string Email,
    string FirstName,
    string LastName,
    string Token,
    int ExpiresIn
    );
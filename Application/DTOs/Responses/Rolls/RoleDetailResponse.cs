using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Responses.Rolls;

public record RoleDetailResponse(
    string Id,
    string Name,
    bool IsDeleted,
    IEnumerable<string> Permissions
);
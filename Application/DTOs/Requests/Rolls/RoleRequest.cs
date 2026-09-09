using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Requests.Rolls;

public record RoleRequest(
    string Name,
    IList<string> Permissions
);
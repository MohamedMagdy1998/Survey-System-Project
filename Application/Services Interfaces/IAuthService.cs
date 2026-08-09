using Application.DTOs.Responses.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services_Interfaces;

public interface IAuthService
{
    public Task<AuthResponse?> GetTokenAsync(string username, string password,CancellationToken cancellationToken=default);
}

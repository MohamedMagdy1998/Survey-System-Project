using Application.DTOs.Requests.Authorization;
using Application.DTOs.Responses.Authorization;
using Domain.Common.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services_Interfaces;

public interface IAuthService
{
    public Task<Result> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    public Task<Result> ConfirmEmailAsync(ConfirmEmailRequest request);

    public Task<Result> ResendConfirmationEmailAsync(ResendConfirmationEmailRequest request);

    public Task<Result<AuthResponse>> GetTokenAsync(string username, string password,CancellationToken cancellationToken=default);

    public Task<Result<AuthResponse>> GetNewTokenAndRefreshTokenAsync(string token, string refreshToken,
    CancellationToken cancellationToken = default);

    public Task<Result> RevokeRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default);
}

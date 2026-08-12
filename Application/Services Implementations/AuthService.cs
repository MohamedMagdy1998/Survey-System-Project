using Application.DTOs.Responses.Authorization;
using Application.Services_Interfaces;
using Domain.Common.Abstractions;
using Domain.Common.Abstractions.Errors;
using Domain.Common.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services_Implementations;

public class AuthService : IAuthService
{
    private readonly IJwtProvider _jwtProvider;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly int refreshTokenExpiryDate = 14;
    public AuthService(IJwtProvider jwtProvider, UserManager<ApplicationUser> userManager)
    {
        _jwtProvider = jwtProvider;
        _userManager = userManager;
    }
    public async Task<Result<AuthResponse>> GetTokenAsync(string email, string password, CancellationToken cancellationToken)
    {

        var user = await _userManager.FindByEmailAsync(email);
       
        if(user is null)
        {
            return UserErrors.InvalidCredentials;
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);
        if(!isPasswordValid) 
        {
            return UserErrors.InvalidCredentials;
        }

        var (token, expiresIn) = _jwtProvider.GenerateToken(user);

        var refreshToken = GenerateRefreshToken();
        var refreshTokenExpirattion = DateTime.UtcNow.AddDays(refreshTokenExpiryDate);

        user.RefreshTokens.Add(new RefreshToken
        {
            Token = refreshToken,
            ExpiresOn = refreshTokenExpirattion
        });

        await _userManager.UpdateAsync(user);

        var response = new AuthResponse
        (
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName,
            token,
            expiresIn,
            refreshToken,
            refreshTokenExpirattion
        );

        return response;
    }

    public async Task<Result<AuthResponse>> GetNewTokenAndRefreshTokenAsync(string token, string refreshToken,
CancellationToken cancellationToken = default)
    {
       
        var userId = _jwtProvider.ValidateToken(token);
                   
                    if (userId is null)
                    {
            return UserErrors.InvalidCredentials;
                    }

        var user = await _userManager.FindByIdAsync(userId);

                    if (user == null)
                    {
                             return UserErrors.NotFound;
                    }

        var userRefreshToken = user.RefreshTokens.SingleOrDefault(rt =>
rt.Token == refreshToken && rt.IsActive);
                    
                    if (userRefreshToken == null)
                    {
                        return UserErrors.InvalidCredentials;
                    }

        userRefreshToken.RevokedOn = DateTime.UtcNow;

        var (newToken, newExpiresIn) = _jwtProvider.GenerateToken(user);


        var newRefreshToken = GenerateRefreshToken();
        var newRefreshTokenExpiration = DateTime.UtcNow.AddDays(refreshTokenExpiryDate);


        user.RefreshTokens.Add(new RefreshToken
        {
            Token = newRefreshToken,
            ExpiresOn = newRefreshTokenExpiration
        });

        await _userManager.UpdateAsync(user);

        var response = new AuthResponse
        (
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email!,
            newToken,
            newExpiresIn,
            newRefreshToken!,
            newRefreshTokenExpiration
        );

        return response;
    }

    public async Task<Result> RevokeRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default)
    {
        var userId = _jwtProvider.ValidateToken(token);
        if (userId == null)
        {
            return UserErrors.NotFound;
        }
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return UserErrors.NotFound;
        }
        var userRefreshToken = user.RefreshTokens.SingleOrDefault(rt => rt.Token == refreshToken);
        if (userRefreshToken == null)
        {
            return UserErrors.NotFound;
        }
        userRefreshToken.RevokedOn = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);


        return Result.Success();
    }

    private static string GenerateRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));



}

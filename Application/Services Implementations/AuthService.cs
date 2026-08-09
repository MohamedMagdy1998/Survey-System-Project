using Application.DTOs.Responses.Authorization;
using Application.Services_Interfaces;
using Domain.Common.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services_Implementations;

public class AuthService : IAuthService
{
    private readonly IJwtProvider _jwtProvider;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthService(IJwtProvider jwtProvider, UserManager<ApplicationUser> userManager)
    {
        _jwtProvider = jwtProvider;
        _userManager = userManager;
    }
    public async Task<AuthResponse?> GetTokenAsync(string email, string password, CancellationToken cancellationToken)
    {
        // Check user

        var user = await _userManager.FindByEmailAsync(email);
        if(user is null)
        {
            return null;
        }
        // check password
        var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);
        if(!isPasswordValid) 
        {
            return null;
        }
        // Generate token
        var (token, expiresIn) = _jwtProvider.GenerateToken(user);

        return new AuthResponse
        (
             user.Id,
             user.Email!,
             user.FirstName, 
             user.LastName, 
            token,
            expiresIn 
        );
    }
}

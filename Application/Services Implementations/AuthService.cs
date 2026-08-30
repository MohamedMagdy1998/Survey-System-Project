using Application.DTOs.Requests.Authorization;
using Application.DTOs.Responses.Authorization;
using Application.Helpers;
using Application.Services_Interfaces;
using Domain.Common.Abstractions;
using Domain.Common.Abstractions.Errors;
using Domain.Common.Const;
using Domain.Common.Interfaces;
using Domain.Contracts;
using Domain.Models;
using Hangfire;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<AuthService> Logger;
    private readonly IEmailSender EmailSender;
    private readonly IHttpContextAccessor HttpContextAccessor;
    private readonly IUnitOfWork UnitOfWork;
    private readonly int refreshTokenExpiryDate = 14;
    public AuthService(
                        IJwtProvider jwtProvider,
                        UserManager<ApplicationUser> userManager,
                        ILogger<AuthService> logger,
                         IEmailSender emailSender,
                        IHttpContextAccessor httpContextAccessor,
                        IUnitOfWork unitOfWork
                        
                      )
    {
        _jwtProvider = jwtProvider;
        _userManager = userManager;
        Logger = logger;
        EmailSender = emailSender;
        HttpContextAccessor = httpContextAccessor;
        UnitOfWork = unitOfWork;
    }


    public async Task<Result> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var emailIsExists = await _userManager.Users.AnyAsync(x => x.Email == request.Email, cancellationToken);

        if (emailIsExists)
            return UserErrors.DuplicateEmail;

        var user = request.Adapt<ApplicationUser>();
        user.UserName = request.Email;

        var result = await _userManager.CreateAsync(user, request.Password);

        if (result.Succeeded)
        {
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            Logger.LogInformation("Confirmation code: {code}", code);

            await SendConfirmationEmail(user, code);

            return Result.Success();
        }

        var error = result.Errors.First();

        return Error.Custom(error.Code, error.Description, StatusCodes.Status400BadRequest);
    }

    public async Task<Result> ConfirmEmailAsync(ConfirmEmailRequest request)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
      
        if (user is null)
            return UserErrors.InvalidCode;

        if (user.EmailConfirmed)
            return UserErrors.DuplicatedConfirmation;

        var code = request.Code;

        try
        {
            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        }
        catch (FormatException)
        {
            return UserErrors.InvalidCode;
        }

        var result = await _userManager.ConfirmEmailAsync(user, code);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, DefaultRoles.Member);
            return Result.Success();

        }
        

        var error = result.Errors.First();

        return Error.Custom(error.Code, error.Description, StatusCodes.Status400BadRequest);
    }


    public async Task<Result> ResendConfirmationEmailAsync(ResendConfirmationEmailRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null )
            return Result.Success();

        if (user.EmailConfirmed)
            return (UserErrors.DuplicatedConfirmation);

        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        Logger.LogInformation("Confirmation code: {code}", code);

        await SendConfirmationEmail(user, code);

        return Result.Success();
    }

    private async Task SendConfirmationEmail(ApplicationUser user, string code)
    {
        var origin = HttpContextAccessor.HttpContext?.Request.Headers.Origin;

        var emailBody = EmailBodyBuilder.GenerateEmailBody("EmailConfirmation",
            templateModel: new Dictionary<string, string>
            {
                { "{{name}}", user.FirstName },
                    { "{{action_url}}", $"{origin}/auth/emailConfirmation?userId={user.Id}&code={code}" }
            }
        );

        BackgroundJob.Enqueue(() => EmailSender.SendEmailAsync(user.Email!, "✅ Survey Basket: Email Confirmation", emailBody));

        await Task.CompletedTask;
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

        var (userRoles, userPermissions) = await UnitOfWork.Roles.GetUserRolesAndPermissions(user, cancellationToken);
        var (newToken, expiresIn) = _jwtProvider.GenerateToken(user, userRoles, userPermissions);

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
            newToken,
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

        var (userRoles, userPermissions) = await UnitOfWork.Roles.GetUserRolesAndPermissions(user, cancellationToken);


        var (newToken, expiresIn) = _jwtProvider.GenerateToken(user, userRoles, userPermissions);


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
            expiresIn,
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


    public async Task<Result> SendResetPasswordCodeAsync(string email)
    {
        if (await _userManager.FindByEmailAsync(email) is not { } user)
            return Result.Success();

        if (!user.EmailConfirmed)
            return Result.Failure(UserErrors.EmailNotConfirmed);

        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        Logger.LogInformation("Reset code: {code}", code);

        await SendResetPasswordEmail(user, code);

        return Result.Success();
    }


    private async Task SendResetPasswordEmail(ApplicationUser user, string code)
    {
        var origin = HttpContextAccessor.HttpContext?.Request.Headers.Origin;

        var emailBody = EmailBodyBuilder.GenerateEmailBody("ForgetPassword",
            templateModel: new Dictionary<string, string>
            {
                { "{{name}}", user.FirstName },
                { "{{action_url}}", $"{origin}/auth/forgetPassword?email={user.Email}&code={code}" }
            }
        );

        BackgroundJob.Enqueue(() => EmailSender.SendEmailAsync(user.Email!, "✅ Survey Basket: Change Password", emailBody));

        await Task.CompletedTask;
    }

    public async Task<Result> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null || !user.EmailConfirmed)
            return Result.Failure(UserErrors.InvalidCode);

        IdentityResult result;

        try
        {
            var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Code));
            result = await _userManager.ResetPasswordAsync(user, code, request.NewPassword);
        }
        catch (FormatException)
        {
            result = IdentityResult.Failed(_userManager.ErrorDescriber.InvalidToken());
        }

        if (result.Succeeded)
            return Result.Success();

        var error = result.Errors.First();

        return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status401Unauthorized));
    }





    private static string GenerateRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    //private async Task<(IEnumerable<string> roles, IEnumerable<string> permissions)> GetUserRolesAndPermissions(ApplicationUser user, CancellationToken cancellationToken)
    //{
    //    var userRoles = await _userManager.GetRolesAsync(user);

    //    //var userPermissions = await _context.Roles
    //    //    .Join(_context.RoleClaims,
    //    //        role => role.Id,
    //    //        claim => claim.RoleId,
    //    //        (role, claim) => new { role, claim }
    //    //    )
    //    //    .Where(x => userRoles.Contains(x.role.Name!))
    //    //    .Select(x => x.claim.ClaimValue!)
    //    //    .Distinct()
    //    //    .ToListAsync(cancellationToken);

    //    var userPermissions = await (from r in _context.Roles
    //                                 join p in _context.RoleClaims
    //                                 on r.Id equals p.RoleId
    //                                 where userRoles.Contains(r.Name!)
    //                                 select p.ClaimValue!)
    //                                 .Distinct()
    //                                 .ToListAsync(cancellationToken);

    //    return (userRoles, userPermissions);
    //}



}

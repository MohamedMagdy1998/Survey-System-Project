using Application.DTOs.Requests.Users;
using Application.DTOs.Responses.Users;
using Application.Services_Interfaces;
using Domain.Common.Abstractions;
using Domain.Common.Abstractions.Errors;
using Domain.Common.Const;
using Domain.Contracts;
using Domain.Models;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services_Implementations;

public class UserService(
    UserManager<ApplicationUser> userManager,
    IRoleService roleService,
    IUnitOfWork unitOfWork) : IUserService
{
    public async Task<IEnumerable<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var usersWithRoles = await unitOfWork.Users.GetAllUsersWithRolesAsync(cancellationToken);

        return usersWithRoles.Select(entry => new UserResponse(
            entry.User.Id,
            entry.User.FirstName,
            entry.User.LastName,
            entry.User.Email!,
            entry.User.IsDisabled,
            entry.Roles
        ));
    }

    public async Task<Result<UserResponse>> GetAsync(string id)
    {
        if (await userManager.FindByIdAsync(id) is not { } user)
            return UserErrors.NotFound;

        var userRoles = await userManager.GetRolesAsync(user);

        return new UserResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email!,
            user.IsDisabled,
            userRoles
        );
    }

    public async Task<Result<UserResponse>> AddAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var emailExists = await userManager.Users.AnyAsync(x => x.Email == request.Email, cancellationToken);
        if (emailExists)
            return UserErrors.DuplicateEmail;

        var allowedRoles = await roleService.GetAllAsync(cancellationToken: cancellationToken);
        if (request.Roles.Except(allowedRoles.Select(x => x.Name)).Any())
            return UserErrors.InvalidCredentials;

        var user = request.Adapt<ApplicationUser>();
        user.UserName = request.Email;

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var error = result.Errors.First();
            return Error.Custom(error.Code, error.Description, StatusCodes.Status400BadRequest);
        }

        await userManager.AddToRolesAsync(user, request.Roles);

        return new UserResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email!,
            user.IsDisabled,
            request.Roles
        );
    }

    public async Task<Result> UpdateAsync(string id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var emailExists = await userManager.Users.AnyAsync(x => x.Email == request.Email && x.Id != id, cancellationToken);
        if (emailExists)
            return UserErrors.DuplicateEmail;

        var allowedRoles = await roleService.GetAllAsync(cancellationToken: cancellationToken);
        if (request.Roles.Except(allowedRoles.Select(x => x.Name)).Any())
            return UserErrors.InvalidCredentials;

        if (await userManager.FindByIdAsync(id) is not { } user)
            return UserErrors.NotFound;

        user = request.Adapt(user);

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var error = result.Errors.First();
            return Error.Custom(error.Code, error.Description, StatusCodes.Status400BadRequest);
        }

        await unitOfWork.Users.RemoveUserRolesAsync(id, cancellationToken);
        await userManager.AddToRolesAsync(user, request.Roles);

        return Result.Success();
    }

    public async Task<Result> ToggleStatusAsync(string id)
    {
        if (await userManager.FindByIdAsync(id) is not { } user)
            return UserErrors.NotFound;

        user.IsDisabled = !user.IsDisabled;
        var result = await userManager.UpdateAsync(user);

        if (result.Succeeded)
            return Result.Success();

        var error = result.Errors.First();
        return Error.Custom(error.Code, error.Description, StatusCodes.Status400BadRequest);
    }

    public async Task<Result> UnlockAsync(string id)
    {
        if (await userManager.FindByIdAsync(id) is not { } user)
            return UserErrors.NotFound;

        var result = await userManager.SetLockoutEndDateAsync(user, null);

        if (result.Succeeded)
            return Result.Success();

        var error = result.Errors.First();
        return Error.Custom(error.Code, error.Description, StatusCodes.Status400BadRequest);
    }
    public async Task<Result<UserProfileResponse>> GetProfileAsync(string userId)
    {
        var user = await userManager.Users
            .Where(x => x.Id == userId)
            .ProjectToType<UserProfileResponse>()
            .SingleAsync();

        return Result.Success(user);
    }
    public async Task<Result> UpdateProfileAsync(string userId, UpdateProfileRequest request)
    {
        //var user = await _userManager.FindByIdAsync(userId);

        //user = request.Adapt(user);

        //await _userManager.UpdateAsync(user!);

        await userManager.Users
            .Where(x => x.Id == userId)
            .ExecuteUpdateAsync(setters =>
                setters
                    .SetProperty(x => x.FirstName, request.FirstName)
                    .SetProperty(x => x.LastName, request.LastName)
            );

        return Result.Success();
    }

    public async Task<Result> ChangePasswordAsync(string userId, ChangePasswordRequest request)
    {
        var user = await userManager.FindByIdAsync(userId);

        var result = await userManager.ChangePasswordAsync(user!, request.CurrentPassword, request.NewPassword);

        if (result.Succeeded)
            return Result.Success();

        var error = result.Errors.First();

        return  Error.Custom(error.Code, error.Description, StatusCodes.Status400BadRequest);
    }


}

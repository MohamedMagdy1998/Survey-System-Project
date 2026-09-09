using Application.DTOs.Requests.Rolls;
using Application.DTOs.Responses.Rolls;
using Application.Services_Interfaces;
using Domain.Common.Abstractions;
using Domain.Common.Abstractions.Errors;
using Domain.Common.Const;
using Domain.Contracts;
using Domain.Models;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Application.Services_Implementations;

public class RoleService(
    RoleManager<ApplicationRole> roleManager,
    IUnitOfWork unitOfWork) : IRoleService
{
    public async Task<IEnumerable<RoleResponse>> GetAllAsync(
        bool? includeDisabled = false,
        CancellationToken cancellationToken = default)
    {
        return await roleManager.Roles
            .Where(x => !x.IsDefault && (!x.IsDeleted || (includeDisabled.HasValue && includeDisabled.Value)))
            .ProjectToType<RoleResponse>()
            .ToListAsync(cancellationToken);
    }

    public async Task<Result<RoleDetailResponse>> GetAsync(string id)
    {
        if (await roleManager.FindByIdAsync(id) is not { } role)
            return RoleErrors.RoleNotFound;

        var permissions = await roleManager.GetClaimsAsync(role);

        return new RoleDetailResponse(role.Id, role.Name!, role.IsDeleted, permissions.Select(x => x.Value));
    }

    public async Task<Result<RoleDetailResponse>> AddAsync(
        RoleRequest request,
        CancellationToken cancellationToken = default)
    {
        var roleExists = await roleManager.RoleExistsAsync(request.Name);
        if (roleExists)
            return RoleErrors.DuplicatedRole;

        var allowedPermissions = Permissions.GetAllPermissions();
        if (request.Permissions.Except(allowedPermissions).Any())
            return RoleErrors.InvalidPermissions;

        var role = new ApplicationRole
        {
            Name = request.Name,
            ConcurrencyStamp = Guid.NewGuid().ToString()
        };

        var result = await roleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            var error = result.Errors.First();
            return Error.Custom(error.Code, error.Description, StatusCodes.Status400BadRequest);
        }

        await unitOfWork.Roles.AddPermissionsAsync(role.Id, request.Permissions, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        return new RoleDetailResponse(role.Id, role.Name, role.IsDeleted, request.Permissions);
    }

    public async Task<Result> UpdateAsync(
        string id,
        RoleRequest request,
        CancellationToken cancellationToken = default)
    {
        var roleExists = await roleManager.Roles.AnyAsync(x => x.Name == request.Name && x.Id != id, cancellationToken);
        if (roleExists)
            return RoleErrors.DuplicatedRole;

        if (await roleManager.FindByIdAsync(id) is not { } role)
            return RoleErrors.RoleNotFound;

        var allowedPermissions = Permissions.GetAllPermissions();
        if (request.Permissions.Except(allowedPermissions).Any())
            return RoleErrors.InvalidPermissions;

        role.Name = request.Name;

        var result = await roleManager.UpdateAsync(role);
        if (!result.Succeeded)
        {
            var error = result.Errors.First();
            return Error.Custom(error.Code, error.Description, StatusCodes.Status400BadRequest);
        }

        await unitOfWork.Roles.UpdatePermissionsAsync(id, request.Permissions, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> ToggleStatusAsync(string id)
    {
        if (await roleManager.FindByIdAsync(id) is not { } role)
            return RoleErrors.RoleNotFound;

        role.IsDeleted = !role.IsDeleted;
        await roleManager.UpdateAsync(role);

        return Result.Success();
    }
}
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Users.DTOs;
using Domain.Common;
using Domain.Repositories;
using Domain.ValueObjects;
using MediatR;

namespace Application.Features.Users.Queries.GetUsers;

public sealed class GetUsersQueryHandler(
    IUserRepository users,
    ITenantContext tenantContext) : IRequestHandler<GetUsersQuery, Result<PaginatedList<UserDto>>>
{
    public async Task<Result<PaginatedList<UserDto>>> Handle(GetUsersQuery request, CancellationToken ct)
    {
        if (!tenantContext.IsResolved)
        {
            return Result<PaginatedList<UserDto>>.Failure(Error.Validation("Tenant context could not be resolved."));
        }

        var tenantIdResult = TenantId.Create(tenantContext.TenantId);
        if (tenantIdResult.IsFailure)
        {
            return Result<PaginatedList<UserDto>>.Failure(tenantIdResult.Error);
        }

        var allUsers = await users.GetByTenantAsync(tenantIdResult.Value, ct);

        var filtered = request.IsActive.HasValue
            ? allUsers.Where(u => u.IsActive == request.IsActive.Value).ToList()
            : allUsers.ToList();

        var totalCount = filtered.Count;
        var pageSize = Math.Max(1, request.PageSize);
        var pageNumber = Math.Max(1, request.PageNumber);

        var items = filtered
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserDto(
                Id: u.Id,
                Email: u.Email.Value,
                FullName: u.FullName,
                IsActive: u.IsActive,
                CreatedAt: u.CreatedAtUtc))
            .ToList();

        return Result<PaginatedList<UserDto>>.Success(
            new PaginatedList<UserDto>(items, totalCount, pageNumber, pageSize));
    }
}

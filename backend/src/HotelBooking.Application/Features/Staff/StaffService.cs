using FluentValidation;
using HotelBooking.Application.Common.Exceptions;
using HotelBooking.Application.Common.Interfaces;
using HotelBooking.Application.Features.Staff.DTOs;
using HotelBooking.Domain.Constants;
using HotelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelBooking.Application.Features.Staff;

public sealed class StaffService(
    IAppDbContext db,
    IPasswordHasher passwordHasher,
    IValidator<CreateStaffRequest> createValidator,
    IValidator<UpdateStaffRequest> updateValidator,
    IValidator<StaffFilterRequest> filterValidator,
    IAuditService auditService,
    ILogger<StaffService> logger) : IStaffService
{
    private static readonly string[] AssignableRoles =
        [RoleNames.Manager, RoleNames.Receptionist, RoleNames.Housekeeper];

    private static readonly string[] StaffRoles =
        [RoleNames.Admin, RoleNames.Manager, RoleNames.Receptionist, RoleNames.Housekeeper];

    public async Task<StaffPagedResult> GetAllAsync(
        StaffFilterRequest filter,
        CancellationToken cancellationToken = default)
    {
        await filterValidator.ValidateAndThrowAsync(filter, cancellationToken);

        var query = db.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .Where(u => StaffRoles.Contains(u.Role.Name))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.RoleName))
        {
            var normalized = RoleNames.Normalize(filter.RoleName);
            query = query.Where(u => u.Role.Name == normalized);
        }

        if (filter.IsActive.HasValue)
            query = query.Where(u => u.IsActive == filter.IsActive.Value);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim().ToLower();
            query = query.Where(u =>
                u.FullName.ToLower().Contains(term) ||
                u.Email.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(u => u.Role.Name)
            .ThenBy(u => u.FullName)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(u => new StaffResponse(u.Id, u.FullName, u.Email, u.Role.Name, u.IsActive, u.CreatedAt))
            .ToListAsync(cancellationToken);

        return new StaffPagedResult(items, totalCount, filter.Page, filter.PageSize);
    }

    public async Task<StaffResponse> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var user = await db.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == id && StaffRoles.Contains(u.Role.Name), cancellationToken)
            ?? throw new NotFoundException("Staff member", id);

        return ToResponse(user);
    }

    public async Task<StaffResponse> CreateAsync(
        CreateStaffRequest request,
        CancellationToken cancellationToken = default)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var normalizedRole = RoleNames.Normalize(request.RoleName);

        if (!AssignableRoles.Contains(normalizedRole))
            throw new BadRequestException(
                $"Cannot create staff with role '{request.RoleName}'. Allowed roles: {string.Join(", ", AssignableRoles)}.");

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var emailExists = await db.Users
            .AnyAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (emailExists)
            throw new ConflictException($"A user with email '{normalizedEmail}' already exists.");

        var role = await db.Roles
            .FirstOrDefaultAsync(r => r.Name == normalizedRole, cancellationToken)
            ?? throw new NotFoundException($"Role '{normalizedRole}' was not found.");

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = normalizedEmail,
            PasswordHash = passwordHasher.Hash(request.Password),
            RoleId = role.Id,
            Role = role,
            IsActive = true
        };

        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync("Staff", user.Id, "Created", null,
            $"Email={user.Email} Role={role.Name}",
            cancellationToken);

        logger.LogInformation(
            "Staff member created. UserId={UserId} Email={Email} Role={Role}",
            user.Id, user.Email, role.Name);

        return ToResponse(user);
    }

    public async Task<StaffResponse> UpdateAsync(
        Guid id,
        UpdateStaffRequest request,
        CancellationToken cancellationToken = default)
    {
        await updateValidator.ValidateAndThrowAsync(request, cancellationToken);

        var user = await db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == id && StaffRoles.Contains(u.Role.Name), cancellationToken)
            ?? throw new NotFoundException("Staff member", id);

        if (request.FullName is not null)
            user.FullName = request.FullName.Trim();

        if (request.Email is not null)
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            if (normalizedEmail != user.Email)
            {
                var emailExists = await db.Users
                    .AnyAsync(u => u.Email == normalizedEmail, cancellationToken);
                if (emailExists)
                    throw new ConflictException($"A user with email '{normalizedEmail}' already exists.");
                user.Email = normalizedEmail;
            }
        }

        if (request.RoleName is not null)
        {
            var normalizedRole = RoleNames.Normalize(request.RoleName);

            if (!AssignableRoles.Contains(normalizedRole))
                throw new BadRequestException(
                    $"Cannot assign role '{request.RoleName}'. Allowed roles: {string.Join(", ", AssignableRoles)}.");

            var role = await db.Roles
                .FirstOrDefaultAsync(r => r.Name == normalizedRole, cancellationToken)
                ?? throw new NotFoundException($"Role '{normalizedRole}' was not found.");

            user.RoleId = role.Id;
            user.Role = role;
        }

        await db.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync("Staff", user.Id, "Updated", null,
            $"Email={user.Email} Role={user.Role.Name}",
            cancellationToken);

        logger.LogInformation(
            "Staff member updated. UserId={UserId} Email={Email} Role={Role}",
            user.Id, user.Email, user.Role.Name);

        return ToResponse(user);
    }

    public async Task<StaffResponse> SetActiveAsync(
        Guid id,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var user = await db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == id && StaffRoles.Contains(u.Role.Name), cancellationToken)
            ?? throw new NotFoundException("Staff member", id);

        if (user.Role.Name == RoleNames.Admin)
            throw new BadRequestException("Admin accounts cannot be deactivated through this endpoint.");

        user.IsActive = isActive;
        await db.SaveChangesAsync(cancellationToken);
        await auditService.LogAsync("Staff", user.Id, isActive ? "Activated" : "Deactivated", null, null, cancellationToken);

        logger.LogInformation(
            "Staff member {Action}. UserId={UserId}",
            isActive ? "activated" : "deactivated", id);

        return ToResponse(user);
    }

    public async Task<IReadOnlyList<HousekeeperOption>> GetAssignableHousekeepersAsync(
        CancellationToken cancellationToken = default)
    {
        return await db.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .Where(u => u.Role.Name == RoleNames.Housekeeper && u.IsActive)
            .OrderBy(u => u.FullName)
            .Select(u => new HousekeeperOption(u.Id, u.FullName))
            .ToListAsync(cancellationToken);
    }

    private static StaffResponse ToResponse(User user) =>
        new(user.Id, user.FullName, user.Email, user.Role.Name, user.IsActive, user.CreatedAt);
}

using FluentValidation;
using HotelBooking.Application.Common.Exceptions;
using HotelBooking.Application.Common.Interfaces;
using HotelBooking.Application.Features.Auth.DTOs;
using HotelBooking.Domain.Constants;
using HotelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelBooking.Application.Features.Auth;

public sealed class AuthService(
    IAppDbContext db,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    IValidator<RegisterRequest> registerValidator,
    IValidator<LoginRequest> loginValidator,
    ILogger<AuthService> logger) : IAuthService
{
    public async Task<AuthResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        await registerValidator.ValidateAndThrowAsync(request, cancellationToken);

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var emailExists = await db.Users
            .AnyAsync(user => user.Email == normalizedEmail, cancellationToken);

        if (emailExists)
        {
            throw new ConflictException("A user with this email already exists.");
        }

        var normalizedRoleName = RoleNames.Normalize(request.RoleName);

        if (!string.Equals(normalizedRoleName, RoleNames.Guest, StringComparison.Ordinal))
        {
            throw new BadRequestException("Public registration only supports the Guest role.");
        }

        var role = await db.Roles
            .FirstOrDefaultAsync(r => r.Name == normalizedRoleName, cancellationToken)
            ?? throw new NotFoundException($"Role '{normalizedRoleName}' was not found.");

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

        logger.LogInformation(
            "User registered. UserId={UserId} Email={Email} Role={Role}",
            user.Id,
            user.Email,
            role.Name);

        return CreateAuthResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        await loginValidator.ValidateAndThrowAsync(request, cancellationToken);

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedException("This account has been deactivated.");
        }

        logger.LogInformation(
            "User logged in. UserId={UserId} Email={Email} Role={Role}",
            user.Id,
            user.Email,
            user.Role.Name);

        return CreateAuthResponse(user);
    }

    private AuthResponse CreateAuthResponse(User user)
    {
        var token = jwtTokenService.GenerateToken(user);

        return new AuthResponse(
            token.Token,
            token.ExpiresAt,
            user.Id,
            user.FullName,
            user.Email,
            user.Role.Name);
    }
}

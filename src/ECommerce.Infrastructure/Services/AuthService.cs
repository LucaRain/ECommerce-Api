using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ECommerce.Application.DTOs.Auth;
using ECommerce.Application.Entities;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Interfaces;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ECommerce.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var exists = await _context.Users.AnyAsync(u => u.Email == request.Email);
        if (exists)
            throw new BadRequestException("Email already registered");

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = "Customer",
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return await BuildAuthResponseAsync(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid email or password");

        return await BuildAuthResponseAsync(user);
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var refreshToken = await _context
            .RefreshTokens.Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

        if (refreshToken == null)
            throw new UnauthorizedException("Invalid refresh token");

        if (refreshToken.IsRevoked)
            throw new UnauthorizedException("Refresh token has been revoked");

        if (refreshToken.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedException("Refresh token has expired");

        // revoke old refresh token
        refreshToken.IsRevoked = true;

        // issue new tokens
        var response = await BuildAuthResponseAsync(refreshToken.User);
        await _context.SaveChangesAsync();

        return response;
    }

    public async Task RevokeTokenAsync(RefreshTokenRequest request)
    {
        var refreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt =>
            rt.Token == request.RefreshToken
        );

        if (refreshToken == null)
            throw new NotFoundException("Refresh token not found");

        refreshToken.IsRevoked = true;
        await _context.SaveChangesAsync();
    }

    private async Task<AuthResponse> BuildAuthResponseAsync(User user)
    {
        var (token, expiry) = GenerateJwtToken(user);
        var refreshToken = await GenerateRefreshTokenAsync(user.Id);

        return new AuthResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            TokenExpiry = expiry,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
        };
    }

    private (string token, DateTime expiry) GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Secret"]!));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiry = DateTime.UtcNow.AddHours(2); // access token valid for this long

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expiry,
            signingCredentials: creds
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expiry);
    }

    private async Task<string> GenerateRefreshTokenAsync(Guid userId)
    {
        // revoke all existing refresh tokens for this user
        var existingTokens = await _context
            .RefreshTokens.Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync();

        foreach (var t in existingTokens)
            t.IsRevoked = true;

        // generate new refresh token
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = token,
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(30), // refresh token valid for this long
            CreatedAt = DateTime.UtcNow,
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return token;
    }
}

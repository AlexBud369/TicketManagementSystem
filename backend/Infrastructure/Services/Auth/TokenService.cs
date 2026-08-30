using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.DTOs.Auth;
using Application.Exceptions;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Infrastructure.Services.Audit;
using Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services.Auth;

public sealed class TokenService : ITokenService {
    private readonly AppDbContext _dbContext;
    private readonly JwtOptions _jwtOptions;
    private readonly SymmetricSecurityKey _signingKey;
    private readonly AuditLogWriter _auditLogWriter;

    public TokenService(
        AppDbContext dbContext,
        IOptions<JwtOptions> jwtOptions,
        AuditLogWriter auditLogWriter) {
        _dbContext = dbContext;
        _jwtOptions = jwtOptions.Value;
        _auditLogWriter = auditLogWriter;

        if (string.IsNullOrWhiteSpace(_jwtOptions.SigningKey) ||
            _jwtOptions.SigningKey.Length < 32) {
            throw new InvalidOperationException(
                "Jwt:SigningKey must be at least 32 characters.");
        }

        _signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtOptions.SigningKey));
    }

    public TokenResponse GenerateAccessToken(
        Guid userId,
        string email,
        IEnumerable<string> roles) {
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenMinutes);

        var claims = new List<Claim> {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, email)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var credentials = new SigningCredentials(
            _signingKey, SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new TokenResponse {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(jwt),
            AccessTokenExpiresAt = expiresAt
        };
    }

    public string GenerateRefreshToken() {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

    public async Task SaveRefreshTokenAsync(
        Guid userId,
        string refreshToken,
        bool rememberMe,
        CancellationToken cancellationToken = default) {
        var lifetimeDays = rememberMe
            ? _jwtOptions.RememberMeRefreshTokenDays
            : _jwtOptions.RefreshTokenDays;

        _dbContext.RefreshTokens.Add(new RefreshToken {
            UserId = userId,
            TokenHash = TokenHashHelper.Hash(refreshToken),
            ExpiresAt = DateTime.UtcNow.AddDays(lifetimeDays)
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Guid> ValidateRefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default) {
        var tokenHash = TokenHashHelper.Hash(refreshToken);

        var storedToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(
                token => token.TokenHash == tokenHash,
                cancellationToken);

        if (storedToken is null) {
            throw AppException.Unauthorized("Invalid refresh token.");
        }

        if (storedToken.IsRevoked) {
            await RevokeAllUserTokensAsync(storedToken.UserId, cancellationToken);
            throw AppException.Unauthorized(
                "Refresh token reuse detected. Sign in again.");
        }

        if (storedToken.ExpiresAt <= DateTime.UtcNow) {
            throw AppException.Unauthorized("Refresh token has expired.");
        }

        return storedToken.UserId;
    }

    public async Task RevokeRefreshTokenAsync(
        string refreshToken,
        string replacedByToken,
        CancellationToken cancellationToken = default) {
        var tokenHash = TokenHashHelper.Hash(refreshToken);

        var storedToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(
                token => token.TokenHash == tokenHash,
                cancellationToken);

        if (storedToken is null || storedToken.IsRevoked) {
            return;
        }

        storedToken.IsRevoked = true;
        storedToken.RevokedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(replacedByToken)) {
            storedToken.ReplacedByTokenHash = TokenHashHelper.Hash(replacedByToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RevokeAllUserTokensAsync(
        Guid userId,
        CancellationToken cancellationToken = default) {
        var utcNow = DateTime.UtcNow;

        await _dbContext.RefreshTokens
            .Where(token => token.UserId == userId && !token.IsRevoked)
            .ExecuteUpdateAsync(
                updates => updates
                    .SetProperty(token => token.IsRevoked, true)
                    .SetProperty(token => token.RevokedAt, utcNow),
                cancellationToken);

        _auditLogWriter.Append(
            AuditAction.Logout,
            "User",
            userId.ToString(),
            userId);
        await _auditLogWriter.SaveAsync(cancellationToken);
    }
}

using LifeOS.Application.Abstractions.Identity;
using LifeOS.Domain.Entities;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TokenOptions = LifeOS.Application.Options.TokenOptions;

namespace LifeOS.Infrastructure.Services;

public sealed class JwtTokenService : ITokenService
{
    private readonly LifeOSDbContext _dbContext;
    private readonly TokenOptions _tokenOptions;

    public JwtTokenService(
        LifeOSDbContext dbContext,
        IOptions<TokenOptions> tokenOptionsAccessor)
    {
        _dbContext = dbContext;
        _tokenOptions = tokenOptionsAccessor.Value;
    }

    public AccessTokenResult CreateAccessToken(IEnumerable<Claim> claims, User user)
    {
        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenOptions.SecurityKey)),
            SecurityAlgorithms.HmacSha256);

        var now = DateTime.UtcNow;
        DateTime expiration = now.AddMinutes(_tokenOptions.AccessTokenExpiration);
        var claimsList = claims.ToList();
        var jti = Guid.NewGuid();

        claimsList.Add(new Claim(JwtRegisteredClaimNames.Jti, jti.ToString()));
        claimsList.Add(new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

        var token = new JwtSecurityToken(
            issuer: _tokenOptions.Issuer,
            audience: _tokenOptions.Audience,
            expires: expiration,
            claims: claimsList,
            signingCredentials: signingCredentials);

        var permissions = claimsList
            .Where(c => c.Type == "permission")
            .Select(c => c.Value)
            .ToList();

        return new AccessTokenResult(
            new JwtSecurityTokenHandler().WriteToken(token),
            token.ValidTo,
            jti,
            permissions);
    }

    public RefreshTokenResult CreateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        var token = Convert.ToBase64String(randomNumber);
        var expiresAt = DateTime.UtcNow.AddDays(_tokenOptions.RefreshTokenExpirationDays);
        return new RefreshTokenResult(token, expiresAt);
    }

    public async Task<List<Claim>> GetAuthClaims(User user)
    {
        // Get user roles with role names
        var userRoles = await _dbContext.UserRoles
            .AsNoTracking()
            .Where(ur => ur.UserId == user.Id && !ur.IsDeleted)
            .Include(ur => ur.Role)
            .Where(ur => !ur.Role.IsDeleted)
            .Select(ur => ur.Role.Name!)
            .ToListAsync();

        var authClaims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.UserName),
        };

        // Add role claims
        foreach (var roleName in userRoles)
        {
            authClaims.Add(new Claim(ClaimTypes.Role, roleName));
        }

        // Add permission claims
        if (userRoles.Any())
        {
            // Get all role IDs
            var roleIds = await _dbContext.Roles
                .AsNoTracking()
                .Where(r => userRoles.Contains(r.Name) && !r.IsDeleted)
                .Select(r => r.Id)
                .ToListAsync();

            if (roleIds.Any())
            {
                // Get permissions through RolePermissions (RolePermission soft delete desteklemiyor, fiziksel silme kullanılıyor)
                var permissions = await _dbContext.RolePermissions
                    .AsNoTracking()
                    .Where(rp => roleIds.Contains(rp.RoleId))
                    .Include(rp => rp.Permission)
                    .Where(rp => !rp.Permission.IsDeleted)
                    .Select(rp => rp.Permission.Name)
                    .Distinct()
                    .ToListAsync();

                foreach (var permissionName in permissions)
                {
                    authClaims.Add(new Claim("permission", permissionName));
                }
            }
        }

        return authClaims;
    }

}

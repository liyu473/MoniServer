using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace LyuMonion.JwtAuth.Server;

/// <summary>
/// JWT 认证服务实现
/// </summary>
internal class JwtAuthService(JwtAuthOptions options) : IJwtAuthService
{
    /// <summary>
    /// 生成 Token
    /// </summary>
    public string GenerateToken(string userId, string userName, params Claim[] additionalClaims)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, userName),
        };
        claims.AddRange(additionalClaims);

        return GenerateToken([.. claims]);
    }

    /// <summary>
    /// 生成 Token（自定义 Claims）
    /// </summary>
    public string GenerateToken(params Claim[] claims)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SecretKey!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var allClaims = new List<Claim>(claims)
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: allClaims,
            expires: DateTime.UtcNow.AddMinutes(options.ExpiresInMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// 验证 Token
    /// </summary>
    public bool ValidateToken(string token)
    {
        return ValidateTokenWithClaims(token) is not null;
    }

    /// <summary>
    /// 验证 Token 并返回 Claims
    /// </summary>
    public ClaimsPrincipal? ValidateTokenWithClaims(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(options.SecretKey!);

            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = options.Issuer,
                ValidateAudience = true,
                ValidAudience = options.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);

            return principal;
        }
        catch
        {
            return null;
        }
    }
}

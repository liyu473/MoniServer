using System.Security.Claims;

namespace LyuMonion.JwtAuth.Server;

/// <summary>
/// JWT 认证服务接口
/// </summary>
public interface IJwtAuthService
{
    /// <summary>
    /// 生成 Token
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="userName">用户名</param>
    /// <param name="additionalClaims">额外的 Claims</param>
    string GenerateToken(string userId, string userName, params Claim[] additionalClaims);

    /// <summary>
    /// 验证 Token
    /// </summary>
    bool ValidateToken(string token);

    /// <summary>
    /// 验证 Token 并返回 Claims
    /// </summary>
    ClaimsPrincipal? ValidateTokenWithClaims(string token);
}

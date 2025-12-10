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
    /// 生成 Token（自定义 Claims）
    /// </summary>
    /// <param name="claims">所有 Claims</param>
    string GenerateToken(params Claim[] claims);

    /// <summary>
    /// 验证 Token
    /// </summary>
    bool ValidateToken(string token);

    /// <summary>
    /// 验证 Token 并返回 Claims
    /// </summary>
    ClaimsPrincipal? ValidateTokenWithClaims(string token);

    /// <summary>
    /// 验证 Token（异步，支持自定义验证回调）
    /// </summary>
    /// <param name="token">Token</param>
    /// <param name="serviceProvider">服务提供者，用于获取依赖服务</param>
    /// <returns>验证结果上下文，null 表示 Token 本身无效</returns>
    Task<TokenValidatedContext?> ValidateTokenAsync(string token, IServiceProvider serviceProvider);
}

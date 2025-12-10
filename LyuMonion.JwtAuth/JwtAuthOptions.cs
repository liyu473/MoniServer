using System.Security.Claims;

namespace LyuMonion.JwtAuth;

/// <summary>
/// Token 验证上下文
/// </summary>
public class TokenValidatedContext(ClaimsPrincipal principal)
{
    /// <summary>
    /// 验证通过的 ClaimsPrincipal
    /// </summary>
    public ClaimsPrincipal Principal { get; } = principal;

    /// <summary>
    /// 是否验证失败
    /// </summary>
    public bool IsFailed { get; private set; }

    /// <summary>
    /// 失败原因
    /// </summary>
    public string? FailureMessage { get; private set; }

    /// <summary>
    /// 标记验证失败
    /// </summary>
    public void Fail(string message)
    {
        IsFailed = true;
        FailureMessage = message;
    }
}

/// <summary>
/// JWT 认证配置选项
/// </summary>
public class JwtAuthOptions
{
    /// <summary>
    /// 密钥（至少32字符，必填）
    /// </summary>
    public string? SecretKey { get; set; }

    /// <summary>
    /// 签发者
    /// </summary>
    public string? Issuer { get; set; }

    /// <summary>
    /// 接收者
    /// </summary>
    public string? Audience { get; set; }

    /// <summary>
    /// Token 过期时间（分钟），默认60分钟
    /// </summary>
    public int ExpiresInMinutes { get; set; } = 60;

    /// <summary>
    /// 排除验证的服务接口名称
    /// </summary>
    public HashSet<string> ExcludedServices { get; } = [];

    /// <summary>
    /// Token 验证通过后的自定义验证回调（可选）
    /// 可用于检查用户状态、权限等业务逻辑
    /// </summary>
    public Func<TokenValidatedContext, IServiceProvider, Task>? OnTokenValidated { get; set; }

    /// <summary>
    /// 排除指定服务（链式调用）
    /// </summary>
    public JwtAuthOptions ExcludeServices(params string[] serviceNames)
    {
        foreach (var name in serviceNames)
        {
            ExcludedServices.Add(name);
        }
        return this;
    }
}

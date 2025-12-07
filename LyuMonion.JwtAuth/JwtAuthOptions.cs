namespace LyuMonion.JwtAuth;

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

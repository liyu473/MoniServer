using MagicOnion.Server;
using Microsoft.Extensions.DependencyInjection;

namespace LyuMonion.JwtAuth.Server;

/// <summary>
/// 服务端 JWT 扩展方法
/// </summary>
public static class ServerExtensions
{
    /// <summary>
    /// 添加 JWT 认证
    /// </summary>
    /// <example>
    /// builder.Services.AddMagicOnion()
    ///     .AddJwtAuth(options =>
    ///     {
    ///         options.SecretKey = "your-secret-key";
    ///         options.ExcludeServices("IAuthService");
    ///     });
    /// </example>
    public static IMagicOnionServerBuilder AddJwtAuth(
        this IMagicOnionServerBuilder builder,
        Action<JwtAuthOptions>? configure = null)
    {
        var options = new JwtAuthOptions();
        configure?.Invoke(options);

        // 注册服务
        builder.Services.AddSingleton(options);
        builder.Services.AddSingleton<IJwtAuthService, JwtAuthService>();

        // 添加过滤器
        builder.Services.Configure<MagicOnionOptions>(opt =>
        {
            var jwtService = new JwtAuthService(options);
            opt.GlobalFilters.Add(new JwtAuthFilter(jwtService, options.ExcludedServices));
            opt.GlobalStreamingHubFilters.Add(new JwtAuthStreamingHubFilter(jwtService));
        });

        return builder;
    }
}

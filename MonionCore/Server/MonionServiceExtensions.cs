using LyuMonionCore.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace LyuMonionCore.Server;

/// <summary>
/// 服务器端扩展方法
/// </summary>
public static class MonionServiceExtensions
{
    /// <summary>
    /// 添加 Monion 通知服务
    /// </summary>
    /// <example>
    /// builder.Services.AddMonionNotification();
    /// </example>
    public static IServiceCollection AddMonionNotification(this IServiceCollection services)
    {
        services.AddSingleton<INotificationPushService, NotificationPushService>();
        return services;
    }

    /// <summary>
    /// 添加 Monion 通知服务（使用自定义推送服务）
    /// </summary>
    public static IServiceCollection AddMonionNotification<TPushService>(this IServiceCollection services)
        where TPushService : class, INotificationPushService
    {
        services.AddSingleton<INotificationPushService, TPushService>();
        return services;
    }
}

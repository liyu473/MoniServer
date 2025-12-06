using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;

namespace LyuMonionCore.Client;

/// <summary>
/// 客户端 DI 扩展方法
/// </summary>
public static class MonionClientExtensions
{
    /// <summary>
    /// 添加 Monion 客户端服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="serverAddress">服务器地址</param>
    /// <example>
    /// services.AddMonionClient("https://localhost:5001");
    /// </example>
    public static IServiceCollection AddMonionClient(this IServiceCollection services, string serverAddress)
    {
        var channel = GrpcChannel.ForAddress(serverAddress);
        services.AddSingleton(channel);
        services.AddSingleton<IMonionService, MonionService>();
        services.AddSingleton<NotificationClient>();
        return services;
    }

    /// <summary>
    /// 添加 Monion 客户端服务（使用已有的 GrpcChannel）
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="channel">gRPC 通道</param>
    public static IServiceCollection AddMonionClient(this IServiceCollection services, GrpcChannel channel)
    {
        services.AddSingleton(channel);
        services.AddSingleton<IMonionService, MonionService>();
        services.AddSingleton<NotificationClient>();
        return services;
    }
}

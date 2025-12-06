using LyuMonionCore.Client.Handlers;
using LyuMonionCore.Client.Polling;

namespace LyuMonionCore.Client;

/// <summary>
/// NotificationClient 扩展方法
/// </summary>
public static class NotificationClientExtensions
{
    /// <summary>
    /// 监听连接状态变化（同步）
    /// </summary>
    public static NotificationClient OnConnectionStateChanged(this NotificationClient client, Action<bool> handler)
    {
        client.ConnectionStateChangedSync += handler;
        return client;
    }

    /// <summary>
    /// 监听连接状态变化（异步）
    /// </summary>
    public static NotificationClient OnConnectionStateChangedAsync(this NotificationClient client, Func<bool, Task> handler)
    {
        client.ConnectionStateChangedAsync += handler;
        return client;
    }

    /// <summary>
    /// 监听未知消息（同步）
    /// </summary>
    public static NotificationClient OnUnknownMessage(this NotificationClient client, Action<string, byte[]> handler)
    {
        client.UnknownMessageReceivedSync += handler;
        return client;
    }

    /// <summary>
    /// 监听未知消息（异步）
    /// </summary>
    public static NotificationClient OnUnknownMessageAsync(this NotificationClient client, Func<string, byte[], Task> handler)
    {
        client.UnknownMessageReceivedAsync += handler;
        return client;
    }

    /// <summary>
    /// 启用自动重连
    /// </summary>
    /// <param name="client">通知客户端</param>
    /// <param name="maxRetries">最大重试次数（-1 表示无限重试）</param>
    /// <param name="retryInterval">重试间隔</param>
    /// <param name="onReconnectAttempt">重连尝试回调（同步）</param>
    /// <param name="onReconnectAttemptAsync">重连尝试回调（异步）</param>
    /// <param name="onReconnectFailed">重连失败回调（同步）</param>
    /// <param name="onReconnectFailedAsync">重连失败回调（异步）</param>
    /// <returns>自动重连管理器</returns>
    public static AutoReconnectHandler EnableAutoReconnect(
        this NotificationClient client,
        int maxRetries = 5,
        TimeSpan? retryInterval = null,
        Action<int, int>? onReconnectAttempt = null,
        Func<int, int, Task>? onReconnectAttemptAsync = null,
        Action? onReconnectFailed = null,
        Func<Task>? onReconnectFailedAsync = null)
    {
        var handler = new AutoReconnectHandler(client, maxRetries, retryInterval ?? TimeSpan.FromSeconds(3));
        handler.ReconnectAttempt += onReconnectAttempt;
        handler.ReconnectAttemptAsync += onReconnectAttemptAsync;
        handler.ReconnectFailed += onReconnectFailed;
        handler.ReconnectFailedAsync += onReconnectFailedAsync;
        handler.Start();
        return handler;
    }

    /// <summary>
    /// 启用心跳检测
    /// </summary>
    /// <param name="client">通知客户端</param>
    /// <param name="interval">心跳间隔</param>
    /// <param name="onHeartbeat">心跳发送成功回调（同步）</param>
    /// <param name="onHeartbeatAsync">心跳发送成功回调（异步）</param>
    /// <param name="onHeartbeatFailed">心跳失败回调（同步）</param>
    /// <param name="onHeartbeatFailedAsync">心跳失败回调（异步）</param>
    /// <returns>心跳管理器</returns>
    public static HeartbeatHandler EnableHeartbeat(
        this NotificationClient client,
        TimeSpan? interval = null,
        Action? onHeartbeat = null,
        Func<Task>? onHeartbeatAsync = null,
        Action<Exception>? onHeartbeatFailed = null,
        Func<Exception, Task>? onHeartbeatFailedAsync = null)
    {
        var handler = new HeartbeatHandler(client, interval ?? TimeSpan.FromSeconds(30));
        handler.HeartbeatSent += onHeartbeat;
        handler.HeartbeatSentAsync += onHeartbeatAsync;
        handler.HeartbeatFailed += onHeartbeatFailed;
        handler.HeartbeatFailedAsync += onHeartbeatFailedAsync;
        handler.Start();
        return handler;
    }

    /// <summary>
    /// 创建轮询服务
    /// </summary>
    public static PollingService<TService, TResult> CreatePolling<TService, TResult>(
        this IMonionService monion,
        Func<TService, Task<TResult>> fetchFunc,
        TimeSpan interval)
        where TService : MagicOnion.IService<TService>
    {
        return new PollingService<TService, TResult>(monion, fetchFunc, interval);
    }
}

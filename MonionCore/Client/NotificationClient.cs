using Grpc.Net.Client;
using LyuMonionCore.Abstractions;
using LyuMonionCore.Models;
using MagicOnion.Client;
using MessagePack;

namespace LyuMonionCore.Client;

/// <summary>
/// 通知客户端 - 封装连接管理和消息订阅
/// </summary>
public class NotificationClient : INotificationReceiver, IAsyncDisposable
{
    private readonly Dictionary<string, Action<byte[]>> _handlers = [];
    private INotificationHub? _hub;
    private bool _disposed;

    internal GrpcChannel Channel { get; }
    internal string? ClientName { get; private set; }

    /// <summary>
    /// 连接状态
    /// </summary>
    public bool IsConnected => _hub is not null;

    // 同步事件
    internal event Action<bool>? ConnectionStateChangedSync;
    // 异步事件
    internal event Func<bool, Task>? ConnectionStateChangedAsync;

    internal event Action<string, byte[]>? UnknownMessageReceivedSync;
    internal event Func<string, byte[], Task>? UnknownMessageReceivedAsync;

    /// <summary>
    /// 创建通知客户端
    /// </summary>
    public NotificationClient(string serverAddress)
    {
        Channel = GrpcChannel.ForAddress(serverAddress);
    }

    /// <summary>
    /// 创建通知客户端（使用已有的 GrpcChannel）
    /// </summary>
    public NotificationClient(GrpcChannel channel)
    {
        Channel = channel;
    }

    /// <summary>
    /// 注册消息处理器
    /// </summary>
    public NotificationClient On<T>(Action<T> handler)
    {
        _handlers[typeof(T).Name] = data =>
        {
            var obj = MessagePackSerializer.Deserialize<T>(data);
            handler(obj);
        };
        return this;
    }

    /// <summary>
    /// 连接到服务器并加入通知
    /// </summary>
    public async Task ConnectAsync(string clientName)
    {
        if (_hub is not null)
            throw new InvalidOperationException("Already connected. Call DisconnectAsync first.");

        ClientName = clientName;
        await ConnectInternalAsync();
    }

    internal async Task ConnectInternalAsync()
    {
        _hub = await StreamingHubClient.ConnectAsync<INotificationHub, INotificationReceiver>(Channel, this);
        await _hub.JoinAsync(ClientName!);
        await RaiseConnectionStateChangedAsync(true);
    }

    internal async Task WaitForDisconnectAsync()
    {
        if (_hub is null) return;

        try
        {
            await _hub.WaitForDisconnectAsync();
        }
        catch
        {
            // 连接异常断开
        }
        finally
        {
            if (!_disposed)
            {
                _hub = null;
                await RaiseConnectionStateChangedAsync(false);
            }
        }
    }

    internal async Task SendHeartbeatAsync()
    {
        if (_hub is not null)
        {
            await _hub.SendAsync("heartbeat");
        }
    }

    private async Task RaiseConnectionStateChangedAsync(bool connected)
    {
        ConnectionStateChangedSync?.Invoke(connected);

        if (ConnectionStateChangedAsync is not null)
        {
            await ConnectionStateChangedAsync.Invoke(connected);
        }
    }

    /// <summary>
    /// 断开连接
    /// </summary>
    public async Task DisconnectAsync()
    {
        if (_hub is not null)
        {
            await _hub.DisposeAsync();
            _hub = null;
            ClientName = null;
            await RaiseConnectionStateChangedAsync(false);
        }
    }

    /// <summary>
    /// 接收消息（由 MagicOnion 调用）
    /// </summary>
    void INotificationReceiver.OnMessage(NotificationMessage message)
    {
        if (_handlers.TryGetValue(message.Type, out var handler))
        {
            handler(message.Data);
        }
        else
        {
            UnknownMessageReceivedSync?.Invoke(message.Type, message.Data);
            UnknownMessageReceivedAsync?.Invoke(message.Type, message.Data);
        }
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        await DisconnectAsync();
        Channel.Dispose();
        GC.SuppressFinalize(this);
    }
}

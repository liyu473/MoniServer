using System.Collections.Concurrent;
using MagicOnion.Server.Hubs;
using MessagePack;
using MoniShared.Notification;
using ZLogger;

namespace MoniServer.Hub;

/// <summary>
/// 通知 Hub - 支持任意类型的消息推送
/// </summary>
public class NotificationHub(ILogger<NotificationHub> logger)
    : StreamingHubBase<INotificationHub, INotificationReceiver>,
        INotificationHub
{
    private static IGroup<INotificationReceiver>? _globalRoom;
    private static readonly object _lock = new();
    private static readonly ConcurrentDictionary<string, Guid> _clients = new();

    private string _clientName = string.Empty;
    private const string DefaultRoom = "Notifications";

    public async Task JoinAsync(string clientName)
    {
        _clientName = clientName;
        var room = await Group.AddAsync(DefaultRoom);

        lock (_lock)
        {
            _globalRoom = room;
        }

        _clients[clientName] = ConnectionId;
        logger.ZLogInformation($"客户端 [{clientName}] 已订阅通知, ConnectionId: {ConnectionId}");
    }

    public Task SendAsync(string message)
    {
        return Task.CompletedTask;
    }

    protected override ValueTask OnDisconnected()
    {
        _clients.TryRemove(_clientName, out _);
        logger.ZLogInformation($"客户端 [{_clientName}] 断开连接");
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// 推送任意类型数据给所有客户端
    /// </summary>
    public static void PushToAll<T>(T data)
    {
        var message = new NotificationMessage
        {
            Type = typeof(T).Name,
            Data = MessagePackSerializer.Serialize(data)
        };

        lock (_lock)
        {
            _globalRoom?.All.OnMessage(message);
        }
    }

    /// <summary>
    /// 推送任意类型数据给指定客户端
    /// </summary>
    public static bool PushToClient<T>(string clientName, T data)
    {
        if (!_clients.TryGetValue(clientName, out var connectionId))
            return false;

        var message = new NotificationMessage
        {
            Type = typeof(T).Name,
            Data = MessagePackSerializer.Serialize(data)
        };

        lock (_lock)
        {
            _globalRoom?.Single(connectionId).OnMessage(message);
        }
        return true;
    }

    /// <summary>
    /// 获取所有已连接的客户端名称
    /// </summary>
    public static IEnumerable<string> GetConnectedClients()
    {
        return _clients.Keys;
    }
}

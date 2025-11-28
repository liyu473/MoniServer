using System.Collections.Concurrent;
using MagicOnion.Server.Hubs;
using MessagePack;

namespace LyuMonionCore.Notification;

/// <summary>
/// 通知 Hub 基类 - 服务器端继承此类即可
/// 一个客户端对应一个Hub(Base)实例
/// </summary>
public abstract class NotificationHubBase
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
        OnClientJoined(clientName, ConnectionId);
    }

    public Task SendAsync(string message)
    {
        return Task.CompletedTask;
    }

    protected override ValueTask OnDisconnected()
    {
        _clients.TryRemove(_clientName, out _);
        OnClientDisconnected(_clientName);
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// 客户端加入时回调（可重写）
    /// </summary>
    protected virtual void OnClientJoined(string clientName, Guid connectionId) { }

    /// <summary>
    /// 客户端断开时回调（可重写）
    /// </summary>
    protected virtual void OnClientDisconnected(string clientName) { }

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

    /// <summary>
    /// 检查客户端是否在线
    /// </summary>
    public static bool IsClientConnected(string clientName)
    {
        return _clients.ContainsKey(clientName);
    }
}

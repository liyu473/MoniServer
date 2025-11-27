using System.Collections.Concurrent;
using MagicOnion.Server.Hubs;
using MoniShared.Notification;
using ZLogger;

namespace MoniServer.Hub;

/// <summary>
/// 理解为一个客户端对于一个Hub，各自存自己的 _clientName
/// </summary>
public class NotificationHub(ILogger<NotificationHub> logger)
    : StreamingHubBase<INotificationHub, INotificationReceiver>,
        INotificationHub
{
    /// <summary>
    /// 房间引用，用于广播
    /// </summary>
    private static IGroup<INotificationReceiver>? _globalRoom;
    private static readonly object _lock = new();

    /// <summary>
    /// 客户端名称 -> ConnectionId 的映射
    /// 用于向指定客户端发送消息
    /// </summary>
    private static readonly ConcurrentDictionary<string, Guid> _clients = new();

    private string _clientName = string.Empty;
    private const string DefaultRoom = "Notifications";

    /// <summary>
    /// 客户端连接并订阅通知
    /// </summary>
    public async Task JoinAsync(string clientName)
    {
        _clientName = clientName;
        var room = await Group.AddAsync(DefaultRoom);

        lock (_lock)
        {
            _globalRoom = room;
        }

        // 记录客户端名称和连接ID的映射
        _clients[clientName] = ConnectionId;

        logger.ZLogInformation($"客户端 [{clientName}] 已订阅通知, ConnectionId: {ConnectionId}");
    }

    public Task SendAsync(string message)
    {
        return Task.CompletedTask;
    }

    protected override ValueTask OnDisconnected()
    {
        // 移除客户端映射
        _clients.TryRemove(_clientName, out _);

        logger.ZLogInformation($"客户端 [{_clientName}] 断开连接");
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// 推送消息给所有客户端
    /// </summary>
    public static void PushToAll(string message)
    {
        lock (_lock)
        {
            _globalRoom?.All.OnMessage(message);
        }
    }

    /// <summary>
    /// 推送消息给指定客户端
    /// </summary>
    /// <param name="clientName">客户端名称</param>
    /// <param name="message">消息内容</param>
    /// <returns>是否发送成功（客户端是否存在）</returns>
    public static bool PushToClient(string clientName, string message)
    {
        if (!_clients.TryGetValue(clientName, out var connectionId))
            return false;

        lock (_lock)
        {
            // Single: 只发送给指定的一个客户端
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

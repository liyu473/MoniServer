using LyuMonionCore.Notification;
using ZLogger;

namespace MoniServer.Hub;

/// <summary>
/// 通知 Hub - 继承 MonionCore 的基类
/// </summary>
public class NotificationHub(ILogger<NotificationHub> logger) : NotificationHubBase
{
    protected override void OnClientJoined(string clientName, Guid connectionId)
    {
        logger.ZLogInformation($"客户端 [{clientName}] 已订阅通知, ConnectionId: {connectionId}");
    }

    protected override void OnClientDisconnected(string clientName)
    {
        logger.ZLogInformation($"客户端 [{clientName}] 断开连接");
    }
}

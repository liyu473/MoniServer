using LyuMonionCore.Abstractions;

namespace LyuMonionCore.Server;

/// <summary>
/// 默认通知推送服务实现
/// </summary>
public class NotificationPushService : INotificationPushService
{
    public void PushToAll<T>(T data) => NotificationHubBase.PushToAll(data);

    public bool PushToClient<T>(string clientName, T data) => NotificationHubBase.PushToClient(clientName, data);

    public IEnumerable<string> GetConnectedClients() => NotificationHubBase.GetConnectedClients();

    public bool IsClientConnected(string clientName) => NotificationHubBase.IsClientConnected(clientName);
}

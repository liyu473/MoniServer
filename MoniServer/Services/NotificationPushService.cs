using LyuMonionCore.Notification;

namespace MoniServer.Services;

/// <summary>
/// 通知推送服务实现
/// </summary>
public class NotificationPushService : INotificationPushService
{
    public void PushToAll<T>(T data)
    {
        NotificationHubBase.PushToAll(data);
    }

    public bool PushToClient<T>(string clientName, T data)
    {
        return NotificationHubBase.PushToClient(clientName, data);
    }

    public IEnumerable<string> GetConnectedClients()
    {
        return NotificationHubBase.GetConnectedClients();
    }

    public bool IsClientConnected(string clientName)
    {
        return NotificationHubBase.IsClientConnected(clientName);
    }
}

using MoniServer.Hub;

namespace MoniServer.Services;

/// <summary>
/// 通知推送服务 - 支持任意类型
/// </summary>
public interface INotificationPushService
{
    /// <summary>
    /// 推送任意类型数据给所有客户端
    /// </summary>
    void PushToAll<T>(T data);

    /// <summary>
    /// 推送任意类型数据给指定客户端
    /// </summary>
    bool PushToClient<T>(string clientName, T data);

    /// <summary>
    /// 获取所有已连接的客户端名称
    /// </summary>
    IEnumerable<string> GetConnectedClients();
}

public class NotificationPushService : INotificationPushService
{
    public void PushToAll<T>(T data)
    {
        NotificationHub.PushToAll(data);
    }

    public bool PushToClient<T>(string clientName, T data)
    {
        return NotificationHub.PushToClient(clientName, data);
    }

    public IEnumerable<string> GetConnectedClients()
    {
        return NotificationHub.GetConnectedClients();
    }
}

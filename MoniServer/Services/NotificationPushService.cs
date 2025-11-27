using MoniServer.Hub;

namespace MoniServer.Services;

/// <summary>
/// 通知推送服务
/// </summary>
public interface INotificationPushService
{
    /// <summary>
    /// 推送消息给所有客户端
    /// </summary>
    void PushToAll(string message);

    /// <summary>
    /// 推送消息给指定客户端
    /// </summary>
    /// <param name="clientName">客户端名称（JoinAsync时传入的名称）</param>
    /// <param name="message">消息内容</param>
    /// <returns>是否发送成功</returns>
    bool PushToClient(string clientName, string message);

    /// <summary>
    /// 获取所有已连接的客户端名称
    /// </summary>
    IEnumerable<string> GetConnectedClients();
}

public class NotificationPushService : INotificationPushService
{
    public void PushToAll(string message)
    {
        NotificationHub.PushToAll(message);
    }

    public bool PushToClient(string clientName, string message)
    {
        return NotificationHub.PushToClient(clientName, message);
    }

    public IEnumerable<string> GetConnectedClients()
    {
        return NotificationHub.GetConnectedClients();
    }
}

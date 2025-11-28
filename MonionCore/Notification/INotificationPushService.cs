namespace LyuMonionCore.Notification;

/// <summary>
/// 通知推送服务接口
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

    /// <summary>
    /// 检查客户端是否在线
    /// </summary>
    bool IsClientConnected(string clientName);
}

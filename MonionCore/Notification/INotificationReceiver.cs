namespace LyuMonionCore.Notification;

/// <summary>
/// 通知接收器接口 - 定义服务器可以推送给客户端的方法
/// </summary>
public interface INotificationReceiver
{
    /// <summary>
    /// 接收通用消息（支持任意类型）
    /// </summary>
    void OnMessage(NotificationMessage message);
}

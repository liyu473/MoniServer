namespace MoniShared.Notification;

public interface INotificationReceiver
{
    /// <summary>
    /// 接收通用消息（支持任意类型）
    /// </summary>
    void OnMessage(NotificationMessage message);
}

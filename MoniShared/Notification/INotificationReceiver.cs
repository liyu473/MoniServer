namespace MoniShared.Notification;

public interface INotificationReceiver
{
    void OnMessage(string message);
}

using MoniShared.Notification;
using System.Windows;

namespace MoniClient.Service;

public class NotificationReceiver : INotificationReceiver
{
    public event Action<string>? MessageReceived;

    public void OnMessage(string message)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            MessageReceived?.Invoke(message);
        });
    }
}
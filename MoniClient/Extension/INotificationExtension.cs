using MoniClient.Service;
using MoniShared.Notification;

namespace MoniClient.Extension;

public static class INotificationExtension
{
    public static NotificationReceiver GetReceiver(this INotificationReceiver notification)
    {
        return notification as NotificationReceiver ?? new();
    }
}

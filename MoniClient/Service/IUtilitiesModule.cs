using Jab;
using MonionCore.Notification;

namespace MoniClient.Service;

[ServiceProviderModule]
[Transient<IMonionService, MonionSerrvice>]
[Singleton<INotificationReceiver, NotificationReceiver>]//在客户端内部一个客户端一个单例接收者
internal interface IUtilitiesModule
{
}

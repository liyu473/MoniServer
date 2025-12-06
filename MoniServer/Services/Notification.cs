using Extensions;
using LyuMonionCore.Abstractions;
using MagicOnion;
using MagicOnion.Server;
using MoniShared.SharedDto;
using MoniShared.SharedIService;

namespace MoniServer.Services;

public class Notification(INotificationPushService pushService)
    : ServiceBase<INotification>,
        INotification
{
    public UnaryResult SendMessageFor(string name)
    {
        if (name.IsNullOrEmpty())
        {
            pushService.PushToAll("由服务器主动广播给全部的消息");
        }
        else
            pushService.PushToClient(name, "由服务器主动广播给你的消息");

        return UnaryResult.CompletedResult;
    }

    public UnaryResult SendPersonFor(Person person)
    {
        pushService.PushToAll(person);

        return UnaryResult.CompletedResult;
    }
}

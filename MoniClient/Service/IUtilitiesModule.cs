using Grpc.Net.Client;
using Jab;
using LyuMonionCore.Client;

namespace MoniClient.Service;

[ServiceProviderModule]
[Transient<IMonionService, MonionSerrvice>]
[Singleton(typeof(NotificationClient), Factory = nameof(BuildNotificationClient))]
internal interface IUtilitiesModule
{
    static NotificationClient BuildNotificationClient(GrpcChannel channel) => new(channel);
}

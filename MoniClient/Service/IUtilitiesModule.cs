using Grpc.Net.Client;
using Jab;
using LyuMonionCore.Client;

namespace MoniClient.Service;

[ServiceProviderModule]
[Transient<IMonionService, MonionService>]
[Singleton(typeof(NotificationClient), Factory = nameof(BuildNotificationClient))]
internal interface IUtilitiesModule
{
    static NotificationClient BuildNotificationClient(GrpcChannel channel) => new(channel);
}

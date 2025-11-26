using Jab;

namespace MoniClient.Service;

[ServiceProviderModule]
[Transient<IMonionService, MonionSerrvice>]
internal interface IUtilitiesModule
{
}

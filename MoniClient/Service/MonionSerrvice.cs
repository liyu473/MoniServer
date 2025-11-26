using Grpc.Net.Client;
using MagicOnion;
using MagicOnion.Client;

namespace MoniClient.Service;

public class MonionSerrvice(GrpcChannel channel) : IMonionService
{
    public T Create<T>()
        where T : IService<T>
    {
        return MagicOnionClient.Create<T>(channel);
    }
}

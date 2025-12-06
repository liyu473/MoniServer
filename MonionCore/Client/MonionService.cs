using Grpc.Net.Client;
using MagicOnion;
using MagicOnion.Client;

namespace LyuMonionCore.Client;

/// <summary>
/// MagicOnion 服务工厂实现
/// </summary>
public class MonionService(GrpcChannel channel) : IMonionService
{
    public T Create<T>() where T : IService<T>
    {
        return MagicOnionClient.Create<T>(channel);
    }
}

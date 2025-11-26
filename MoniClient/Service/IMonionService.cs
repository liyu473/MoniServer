using MagicOnion;

namespace MoniClient.Service;

public interface IMonionService
{
    T Create<T>() where T : IService<T>;
}

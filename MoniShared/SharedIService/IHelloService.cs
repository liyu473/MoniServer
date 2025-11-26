using MagicOnion;

namespace MoniShared.SharedIService;

public interface IHelloService : IService<IHelloService>
{
    UnaryResult<string> SayHello(string name);
}

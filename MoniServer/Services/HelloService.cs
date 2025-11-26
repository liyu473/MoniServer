using MagicOnion;
using MagicOnion.Server;
using MoniShared.SharedIService;

namespace MoniServer.Services
{
    public class HelloService:ServiceBase<IHelloService>,IHelloService
    {
        public async UnaryResult<string> SayHello(string name)
        {
            return $"Hello, {name}!";
        }
    }
}

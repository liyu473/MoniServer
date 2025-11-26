using MagicOnion;
using MagicOnion.Server;
using MoniShared.SharedIService;
using ZLogger;

namespace MoniServer.Services
{
    public class CalculatorService(ILogger<CalculatorService> logger) : ServiceBase<ICalculator>, ICalculator
    {
        public async UnaryResult<int> SumAsync(int x, int y)
        {
            logger.ZLogInformation($"Calculating sum: {x} + {y}");
            return x + y;
        }
    }
}

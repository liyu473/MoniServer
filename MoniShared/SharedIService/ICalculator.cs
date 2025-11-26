using MagicOnion;

namespace MoniShared.SharedIService;

public interface ICalculator : IService<ICalculator>
{
    UnaryResult<int> SumAsync(int x, int y);
}

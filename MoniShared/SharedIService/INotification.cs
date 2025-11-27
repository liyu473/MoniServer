using MagicOnion;

namespace MoniShared.SharedIService;

public interface INotification : IService<INotification>
{
    UnaryResult SendMessageFor(string name);
}

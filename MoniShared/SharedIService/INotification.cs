using MagicOnion;
using MoniShared.SharedDto;

namespace MoniShared.SharedIService;

public interface INotification : IService<INotification>
{
    UnaryResult SendMessageFor(string name);
    UnaryResult SendPersonFor(Person person);  // 改名，MagicOnion不支持重载
}

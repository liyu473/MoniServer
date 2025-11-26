using MagicOnion;
using MoniShared.SharedDto;

namespace MoniShared.SharedIService;

public interface IPersonService: IService<IPersonService>
{
    UnaryResult<Person> GetPerson(int id);
}

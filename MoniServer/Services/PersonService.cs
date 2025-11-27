using MagicOnion;
using MagicOnion.Server;
using MoniShared.SharedDto;
using MoniShared.SharedIService;

namespace MoniServer.Services
{
    public class PersonService : ServiceBase<IPersonService>, IPersonService
    {
        public async UnaryResult<Person> GetPerson()
        {
            return new Person()
            {
                Id = 1,
                Name = "hello",
                Age = 18,
            };
        }
    }
}

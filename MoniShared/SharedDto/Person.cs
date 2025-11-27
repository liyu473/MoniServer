using MessagePack;

namespace MoniShared.SharedDto;

[MessagePackObject(true)]
public class Person
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public int Age { get; set; }

    public override string ToString()
    {
        return $"Person(Id={Id}, Name={Name}, Age={Age})";
    }
}


using MessagePack;

namespace MoniShared.SharedDto;

[MessagePackObject]
public class Person
{
    [Key(0)]
    public int Id { get; set; }

    [Key(1)]
    public string Name { get; set; } = "";

    [Key(2)]
    public int Age { get; set; }

    public override string ToString()
    {
        return $"Person(Id={Id}, Name={Name}, Age={Age})";
    }
}

namespace Smurfs.Domain.Encyclopedia;

public class Person
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public required DateTimeOffset CreatedAt { get; init; }
}

namespace Smurfs.Domain;

public class Pet
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public required string Name { get; set; }
}

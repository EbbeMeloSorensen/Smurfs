namespace Smurfs.Domain;

public class Smurf
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public int Age { get; set; }
    public Guid? PetId { get; set; }
    public Pet? Pet { get; set; }
}

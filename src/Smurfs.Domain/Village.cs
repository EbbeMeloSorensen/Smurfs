namespace Smurfs.Domain;

internal class Village
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public ICollection<Smurf> Smurfs { get; } = new List<Smurf>();
}

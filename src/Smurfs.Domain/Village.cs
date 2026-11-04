namespace Smurfs.Domain;

/// <summary>
/// A village model with a mutable collection of Smurfs.
/// </summary>
public class Village
{
    /// <summary>
    /// Gets the identifier assigned when this instance is constructed.
    /// </summary>
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the name. Required initialization does not enforce non-empty text at runtime.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets the mutable membership collection, initially empty.
    /// </summary>
    /// <remarks>
    /// Does not enforce exclusive membership or maintain a reverse Smurf reference.
    /// </remarks>
    public ICollection<Smurf> Smurfs { get; } = new List<Smurf>();
}

namespace Smurfs.Domain;

/// <summary>A named pet that may be shared by multiple Smurfs.</summary>
public class Pet
{
    /// <summary>Gets the identifier assigned when this instance is constructed.</summary>
    public Guid Id { get; private set; } = Guid.NewGuid();
    /// <summary>Gets or sets the name. Required initialization does not enforce non-empty text at runtime.</summary>
    public required string Name { get; set; }
}

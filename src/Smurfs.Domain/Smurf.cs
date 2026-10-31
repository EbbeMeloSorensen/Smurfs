namespace Smurfs.Domain;

/// <summary>A named Smurf with an age and an optional pet.</summary>
public class Smurf
{
    /// <summary>Gets the identifier assigned when this instance is constructed.</summary>
    public Guid Id { get; private set; } = Guid.NewGuid();
    /// <summary>Gets or sets the name. Required initialization does not enforce non-empty text at runtime.</summary>
    public required string Name { get; set; }
    /// <summary>Gets or sets the age; no range validation is applied.</summary>
    public int Age { get; set; }
    /// <summary>Gets or sets the optional pet identifier.</summary>
    /// <remarks>Plain CLR assignment does not synchronize this value with Pet.</remarks>
    public Guid? PetId { get; set; }
    /// <summary>Gets or sets the optional pet reference.</summary>
    /// <remarks>Null can also mean the navigation has not been loaded by persistence.</remarks>
    public Pet? Pet { get; set; }
}

namespace Smurfs.Domain;

/// <summary>
/// A named pet that may be shared by multiple Smurfs.
/// </summary>
public class Pet
{
    /// <summary>
    /// Gets the identifier generated on creation or supplied during rehydration.
    /// </summary>
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the name. Required initialization does not enforce non-empty text at runtime.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Creates a pet with a new identifier.
    /// </summary>
    public Pet()
    {
    }

    /// <summary>
    /// Creates a pet with an existing identifier for import or rehydration.
    /// </summary>
    public Pet(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("The identifier must not be empty.", nameof(id));
        }

        Id = id;
    }
}

using Smurfs.Domain.BusinessRules.AtomicRules;

namespace Smurfs.Domain;

/// <summary>
/// A named Smurf with health, an optional birthday, and an optional pet.
/// </summary>
public class Smurf
{
    private string _name = string.Empty;

    /// <summary>
    /// Gets the identifier generated on creation or supplied during rehydration.
    /// </summary>
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>
    /// Gets the UTC creation timestamp, initialized when the instance is constructed.
    /// Seed and import callers can supply a timestamp during initialization.
    /// </summary>
    public DateTime CreatedUtc { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the nonblank name, which can contain at most 20 characters.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// The assigned name is blank or too long.
    /// </exception>
    public required string Name
    {
        get => _name;
        set
        {
            var violation = NameIsValidRule.ValidateName(value);
            if (violation is not null)
            {
                throw new ArgumentException(violation.Message, nameof(value));
            }

            _name = value;
        }
    }

    /// <summary>
    /// Gets or sets the health; no range validation is applied.
    /// </summary>
    public int Health { get; set; }

    /// <summary>
    /// Gets or sets the birthday, or null when it is unknown.
    /// </summary>
    public DateTime? Birthday { get; set; }

    /// <summary>
    /// Gets or sets whether the Smurf is infected, or null when it is unknown.
    /// </summary>
    public bool? Infected { get; set; }

    /// <summary>
    /// Gets or sets the gender, or null when it is unknown.
    /// </summary>
    public Gender? Gender { get; set; }

    /// <summary>
    /// Gets or sets the optional pet identifier.
    /// </summary>
    /// <remarks>
    /// Plain CLR assignment does not synchronize this value with Pet.
    /// </remarks>
    public Guid? PetId { get; set; }

    /// <summary>
    /// Gets or sets the optional pet reference.
    /// </summary>
    /// <remarks>
    /// Null can also mean the navigation has not been loaded by persistence.
    /// </remarks>
    public Pet? Pet { get; set; }

    /// <summary>
    /// Creates a smurf with a new identifier.
    /// </summary>
    public Smurf()
    {
    }

    /// <summary>
    /// Creates a smurf with an existing identifier for import or rehydration.
    /// </summary>
    public Smurf(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("The identifier must not be empty.", nameof(id));
        }

        Id = id;
    }
}

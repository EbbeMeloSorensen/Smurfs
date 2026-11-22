using Smurfs.Domain;

namespace Smurfs.Application.Smurfs;

/// <summary>
/// Contains the fields projected for a Smurf list item.
/// </summary>
public class SmurfDto
{
    /// <summary>
    /// Gets or sets the Smurf identifier used to break ties between equal names.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the persisted UTC creation timestamp.
    /// </summary>
    public DateTime CreatedUtc { get; set; }

    /// <summary>
    /// Gets or sets the persisted Smurf name.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Gets or sets the persisted health.
    /// </summary>
    public int Health { get; set; }
    /// <summary>
    /// Gets or sets the birthday, or null when unknown.
    /// </summary>
    public DateTime? Birthday { get; set; }

    /// <summary>
    /// Gets or sets infection status, or null when unknown.
    /// </summary>
    public bool? Infected { get; set; }

    /// <summary>
    /// Gets or sets the gender, or null when unknown.
    /// </summary>
    public Gender? Gender { get; set; }

    /// <summary>
    /// Gets or sets the optional pet identifier.
    /// </summary>
    public Guid? PetId { get; set; }
}

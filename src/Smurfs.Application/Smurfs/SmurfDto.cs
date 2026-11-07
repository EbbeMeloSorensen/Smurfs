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
    /// Gets or sets the persisted Smurf name.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Gets or sets the persisted age.
    /// </summary>
    public int Age { get; set; }
}

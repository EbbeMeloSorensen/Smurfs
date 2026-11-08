namespace Smurfs.Domain;

/// <summary>
/// Carries a proposed name that can be checked before a Smurf exists.
/// </summary>
public sealed record SmurfNameDraft(string Name);

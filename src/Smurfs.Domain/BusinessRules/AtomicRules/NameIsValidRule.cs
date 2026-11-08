using Craft.Domain;

namespace Smurfs.Domain.BusinessRules.AtomicRules;

/// <summary>
/// Requires a nonblank Smurf name of at most 20 characters.
/// </summary>
public class NameIsValidRule : IBusinessRule<Smurf>, IBusinessRule<SmurfNameDraft>
{
    /// <summary>
    /// Checks a proposed name before constructing or updating a Smurf.
    /// </summary>
    public static BusinessRuleViolation? ValidateName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return new BusinessRuleViolation(nameof(Smurf.Name), "Name is required");
        }

        if (name.Length > 20)
        {
            return new BusinessRuleViolation(nameof(Smurf.Name), "Name too long (max 20 characters)");
        }

        return null;
    }

    /// <inheritdoc />
    public BusinessRuleViolation? Validate(Smurf smurf) => ValidateName(smurf.Name);

    /// <inheritdoc />
    public BusinessRuleViolation? Validate(SmurfNameDraft draft) => ValidateName(draft.Name);
}

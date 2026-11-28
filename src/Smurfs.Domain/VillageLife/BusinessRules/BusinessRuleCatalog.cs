using Craft.Domain;
using Smurfs.Domain.VillageLife.BusinessRules.AtomicRules;

namespace Smurfs.Domain.VillageLife.BusinessRules;

/// <summary>
/// Provides the business rules registered by the Smurfs domain.
/// </summary>
public class BusinessRuleCatalog : BusinessRuleCatalogBase
{
    /// <summary>
    /// Registers the atomic rule for Smurf names.
    /// </summary>
    public BusinessRuleCatalog()
    {
        var nameRule = new NameIsValidRule();
        RegisterAtomicRule<SmurfNameDraft>(nameRule);
        RegisterAtomicRule<Smurf>(nameRule);
    }
}

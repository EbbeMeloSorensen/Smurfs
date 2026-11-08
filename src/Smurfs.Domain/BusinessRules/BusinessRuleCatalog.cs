using Craft.Domain;
using Smurfs.Domain.BusinessRules.AtomicRules;

namespace Smurfs.Domain.BusinessRules;

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
        RegisterAtomicRule(new NameIsValidRule());
    }
}

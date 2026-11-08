using Craft.Domain;
using Smurfs.Domain.BusinessRules.AtomicRules;

namespace Smurfs.Domain.BusinessRules;

public class BusinessRuleCatalog : BusinessRuleCatalogBase
{
    public BusinessRuleCatalog()
    {
        RegisterAtomicRule(new NameIsValidRule());
    }
}

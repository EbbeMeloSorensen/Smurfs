using Craft.Domain;

namespace Smurfs.Domain.BusinessRules.AtomicRules;

public class NameIsValidRule : IBusinessRule<Smurf>
{
    public string RuleName => "Name";

    public string ErrorMessage { get; private set; } = "";

    public bool Validate(
        Smurf smurf)
    {
        if (string.IsNullOrEmpty(smurf.Name))
        {
            ErrorMessage = "Name is required";
            return false;
        }

        if (smurf.Name.Length > 20)
        {
            ErrorMessage = "Name too long (max 20 characters)";
            return false;
        }

        return true;
    }
}

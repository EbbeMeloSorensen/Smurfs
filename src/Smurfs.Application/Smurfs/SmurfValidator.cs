using FluentValidation;
using Smurfs.Domain;
using Smurfs.Domain.BusinessRules.AtomicRules;

namespace Smurfs.Application.Smurfs;

public class SmurfValidator : AbstractValidator<Smurf>
{
    public SmurfValidator()
    {
        RuleFor(x => x.Name).Custom((name, context) =>
        {
            var violation = NameIsValidRule.ValidateName(name);
            if (violation is not null)
            {
                context.AddFailure(violation.Message);
            }
        });
    }
}

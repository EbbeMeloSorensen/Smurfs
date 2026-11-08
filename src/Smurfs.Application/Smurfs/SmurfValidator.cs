using FluentValidation;
using Smurfs.Domain;
using Smurfs.Domain.BusinessRules.AtomicRules;

namespace Smurfs.Application.Smurfs;

/// <summary>
/// Exposes the domain name policy through FluentValidation.
/// </summary>
public class SmurfValidator : AbstractValidator<Smurf>
{
    /// <summary>
    /// Associates domain name violations with the name property.
    /// </summary>
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

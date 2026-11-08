using FluentValidation;
using Smurfs.Domain;

namespace Smurfs.Application.Smurfs;

public class SmurfValidator : AbstractValidator<Smurf>
{
    public SmurfValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Name).MaximumLength(20);
    }
}

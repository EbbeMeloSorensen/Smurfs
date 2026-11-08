using Craft.Domain;
using FluentValidation;
using MediatR;
using Smurfs.Application.Core;
using Smurfs.Domain;
using Smurfs.Persistence;

namespace Smurfs.Application.Smurfs;

/// <summary>
/// Defines the use case for creating a Smurf.
/// </summary>
public class Create
{
    /// <summary>
    /// Requests persistence of the supplied Smurf.
    /// </summary>
    public class Command : IRequest<Result<Unit>>
    {
        /// <summary>
        /// Gets the Smurf supplied by the caller.
        /// </summary>
        public required Smurf Smurf { get; init; }
    }

    /// <summary>
    /// Adapts Smurf validation for FluentValidation callers.
    /// </summary>
    public class CommandValidator : AbstractValidator<Command>
    {
        /// <summary>
        /// Applies the Smurf validator to the command payload.
        /// </summary>
        public CommandValidator()
        {
            RuleFor(x => x.Smurf).SetValidator(new SmurfValidator());
        }
    }

    /// <summary>
    /// Creates and persists a Smurf in a single unit of work.
    /// </summary>
    public class Handler : IRequestHandler<Command, Result<Unit>>
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IBusinessRuleCatalog _businessRuleCatalog;

        /// <summary>
        /// Uses the supplied factory to open a write session for each command.
        /// </summary>
        public Handler(
            IUnitOfWorkFactory unitOfWorkFactory,
            IBusinessRuleCatalog businessRuleCatalog)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _businessRuleCatalog = businessRuleCatalog;
        }

        /// <summary>
        /// Validates the supplied Smurf and commits it before returning success.
        /// Cancellation and persistence exceptions propagate to the caller.
        /// </summary>
        public async Task<Result<Unit>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var errors = _businessRuleCatalog.ValidateAtomic(request.Smurf);

            if (errors.Count > 0)
            {
                return Result<Unit>.Failure($"Business rule violations detected. First: {errors[0].Message}");
            }

            await using var unitOfWork = _unitOfWorkFactory.CreateUnitOfWork();
            await unitOfWork.Smurfs.AddAsync(request.Smurf, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}

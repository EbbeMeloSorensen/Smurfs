using Craft.Domain;
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
    /// Requests creation of a Smurf with the proposed name.
    /// </summary>
    public class Command : IRequest<Result<Unit>>
    {
        /// <summary>
        /// Gets the proposed name supplied by the caller.
        /// </summary>
        public required string Name { get; init; }
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
        /// Validates the proposed name, constructs a Smurf and commits it before returning success.
        /// Cancellation and persistence exceptions propagate to the caller.
        /// </summary>
        public async Task<Result<Unit>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var errors = _businessRuleCatalog.ValidateAtomic(new SmurfNameDraft(request.Name));

            if (errors.Count > 0)
            {
                return Result<Unit>.Failure(errors[0].Message);
            }

            var smurf = new Smurf { Name = request.Name };
            var entityErrors = _businessRuleCatalog.ValidateAtomic(smurf);
            if (entityErrors.Count > 0)
            {
                return Result<Unit>.Failure(entityErrors[0].Message);
            }

            await using var unitOfWork = _unitOfWorkFactory.CreateUnitOfWork();
            await unitOfWork.Smurfs.AddAsync(smurf, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}

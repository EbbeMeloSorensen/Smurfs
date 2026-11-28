using Craft.Domain;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Smurfs.Application.Core;
using Smurfs.Domain.VillageLife;
using Smurfs.Persistence;

namespace Smurfs.Application.Smurfs;

/// <summary>
/// Defines the use case for editing a Smurf's details.
/// </summary>
public class Edit
{
    /// <summary>
    /// Identifies the Smurf and supplies its replacement details.
    /// </summary>
    public class Command : IRequest<Result<Unit>>
    {
        /// <summary>
        /// Gets the identifier of the existing Smurf.
        /// </summary>
        public required Guid Id { get; init; }

        /// <summary>
        /// Gets the proposed name.
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Gets the replacement health. No health range restriction is applied.
        /// </summary>
        public required int Health { get; init; }

        /// <summary>
        /// Gets the replacement birthday, or null when unknown.
        /// </summary>
        public DateTime? Birthday { get; init; }

        /// <summary>
        /// Gets replacement infection status, or null when unknown.
        /// </summary>
        public bool? Infected { get; init; }

        /// <summary>
        /// Gets the replacement gender, or null when unknown.
        /// </summary>
        public Gender? Gender { get; init; }

    }

    /// <summary>
    /// Validates and edits a tracked Smurf within a single unit of work.
    /// </summary>
    public class Handler : IRequestHandler<Command, Result<Unit>>
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        private readonly IBusinessRuleCatalog _businessRuleCatalog;

        private readonly ILogger<Handler> _logger;

        /// <summary>
        /// Uses the supplied session factory, domain catalog, and optional logger.
        /// </summary>
        public Handler(
            IUnitOfWorkFactory unitOfWorkFactory,
            IBusinessRuleCatalog businessRuleCatalog,
            ILogger<Handler>? logger = null)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _businessRuleCatalog = businessRuleCatalog;
            _logger = logger ?? NullLogger<Handler>.Instance;
        }

        /// <summary>
        /// Returns a failure for an invalid draft, missing Smurf, or entity rule violation.
        /// Persists a valid edit and propagates cancellation and persistence exceptions.
        /// </summary>
        public async Task<Result<Unit>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var errors = _businessRuleCatalog.ValidateAtomic(new SmurfNameDraft(request.Name));
            if (errors.Count > 0)
            {
                _logger.LogDebug("Smurf edit rejected by draft validation.");
                return Result<Unit>.Failure(errors[0].Message);
            }

            await using var unitOfWork = _unitOfWorkFactory.CreateUnitOfWork();
            var smurfs = await unitOfWork.Smurfs.FindAsync(
                [smurf => smurf.Id == request.Id], cancellationToken);
            var smurf = smurfs.SingleOrDefault();
            if (smurf is null)
            {
                _logger.LogDebug("Smurf edit could not find {SmurfId}.", request.Id);
                return Result<Unit>.Failure("Smurf not found.");
            }

            smurf.Name = request.Name;
            smurf.Health = request.Health;
            smurf.Birthday = request.Birthday;
            smurf.Infected = request.Infected;
            smurf.Gender = request.Gender;
            var entityErrors = _businessRuleCatalog.ValidateAtomic(smurf);
            if (entityErrors.Count > 0)
            {
                _logger.LogDebug("Smurf edit rejected by entity validation.");
                return Result<Unit>.Failure(entityErrors[0].Message);
            }

            await unitOfWork.CompleteAsync(cancellationToken);
            _logger.LogInformation("Smurf {SmurfId} edited.", request.Id);
            return Result<Unit>.Success(Unit.Value);
        }
    }
}

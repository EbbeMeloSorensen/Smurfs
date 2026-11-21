using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Smurfs.Application.Core;
using Smurfs.Persistence;

namespace Smurfs.Application.Smurfs;

/// <summary>
/// Defines the use case for deleting a Smurf.
/// </summary>
public class Delete
{
    /// <summary>
    /// Identifies the Smurf to delete.
    /// </summary>
    public class Command : IRequest<Result<Unit>>
    {
        /// <summary>
        /// Gets the identifier of the existing Smurf.
        /// </summary>
        public required Guid Id { get; init; }
    }

    /// <summary>
    /// Deletes an existing Smurf within a single unit of work.
    /// </summary>
    public class Handler : IRequestHandler<Command, Result<Unit>>
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        private readonly ILogger<Handler> _logger;

        /// <summary>
        /// Uses the supplied session factory and optional logger.
        /// </summary>
        public Handler(
            IUnitOfWorkFactory unitOfWorkFactory,
            ILogger<Handler>? logger = null)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _logger = logger ?? NullLogger<Handler>.Instance;
        }

        /// <summary>
        /// Returns a failure if the Smurf is missing and commits its removal otherwise.
        /// Cancellation and persistence exceptions propagate to the caller.
        /// </summary>
        public async Task<Result<Unit>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            await using var unitOfWork = _unitOfWorkFactory.CreateUnitOfWork();
            var smurfs = await unitOfWork.Smurfs.FindAsync(
                [smurf => smurf.Id == request.Id], cancellationToken);
            var smurf = smurfs.SingleOrDefault();
            if (smurf is null)
            {
                _logger.LogDebug("Smurf deletion could not find {SmurfId}.", request.Id);
                return Result<Unit>.Failure("Smurf not found.");
            }

            await unitOfWork.Smurfs.RemoveAsync(smurf, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);
            _logger.LogInformation("Smurf {SmurfId} deleted.", request.Id);
            return Result<Unit>.Success(Unit.Value);
        }
    }
}

using MediatR;
using Smurfs.Domain;
using Smurfs.Persistence;
using Smurfs.Application.Core;

namespace Smurfs.Application.Smurfs;

/// <summary>
/// Defines the use case for creating a Smurf.
/// </summary>
public class Create
{
    /// <summary>
    /// Requests creation of a Smurf using the supplied name.
    /// </summary>
    public class Command : IRequest<Result<Unit>>
    {
        /// <summary>
        /// Gets the name supplied by the caller.
        /// </summary>
        public required string Name { get; init; }
    }

    /// <summary>
    /// Creates and persists a Smurf in a single unit of work.
    /// </summary>
    public class Handler : IRequestHandler<Command, Result<Unit>>
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        /// <summary>
        /// Uses the supplied factory to open a write session for each command.
        /// </summary>
        public Handler(
            IUnitOfWorkFactory unitOfWorkFactory)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        /// <summary>
        /// Creates a Smurf with age zero and commits it before returning success.
        /// Cancellation and persistence exceptions propagate to the caller.
        /// </summary>
        public async Task<Result<Unit>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var smurf = new Smurf { Name = request.Name, Age = 0 };

            await using var unitOfWork = _unitOfWorkFactory.CreateUnitOfWork();
            await unitOfWork.Smurfs.AddAsync(smurf, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}

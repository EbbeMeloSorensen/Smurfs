using MediatR;
using Smurfs.Application.Core;
using Smurfs.Persistence;

namespace Smurfs.Application.Smurfs;

/// <summary>
/// Defines the use case for reading one Smurf.
/// </summary>
public class Details
{
    /// <summary>
    /// Identifies the Smurf to retrieve.
    /// </summary>
    public class Query : IRequest<Result<SmurfDto>>
    {
        /// <summary>
        /// Gets the requested identifier.
        /// </summary>
        public required Guid Id { get; init; }
    }

    /// <summary>
    /// Reads one Smurf through the persistence contract.
    /// </summary>
    public class Handler(IUnitOfWorkFactory unitOfWorkFactory) : IRequestHandler<Query, Result<SmurfDto>>
    {
        /// <inheritdoc />
        public async Task<Result<SmurfDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            await using var unit = unitOfWorkFactory.CreateUnitOfWork();
            var matches = await unit.Smurfs.FindAsync([smurf => smurf.Id == request.Id], cancellationToken);
            var smurf = matches.SingleOrDefault();
            if (smurf is null)
            {
                return Result<SmurfDto>.Failure("Smurf not found.");
            }

            return Result<SmurfDto>.Success(new SmurfDto
            {
                Id = smurf.Id,
                Name = smurf.Name,
                Health = smurf.Health,
                Birthday = smurf.Birthday,
                Infected = smurf.Infected,
                Gender = smurf.Gender,
                PetId = smurf.PetId
            });
        }
    }
}

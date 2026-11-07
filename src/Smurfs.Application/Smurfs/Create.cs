using MediatR;
using Smurfs.Domain;
using Smurfs.Persistence;
using Smurfs.Application.Core;

namespace Smurfs.Application.Smurfs;

public class Create
{
    public class Command : IRequest<Result<Unit>>
    {
        /// <summary>
        /// Gets the name supplied by the caller.
        /// </summary>
        public required string Name { get; init; }
    }

    public class Handler : IRequestHandler<Command, Result<Unit>>
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        public Handler(
            IUnitOfWorkFactory unitOfWorkFactory)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
        }

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

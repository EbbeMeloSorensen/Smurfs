using MediatR;
using Smurfs.Domain;
using Smurfs.Persistence;
using Smurfs.Application.Core;

namespace Smurfs.Application.Smurfs;

public class Create
{
    public class Command : IRequest<Result<Unit>>
    {
        public Smurf Smurf { get; set; }
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
            var unitOfWork = _unitOfWorkFactory.CreateUnitOfWork();

            await unitOfWork.Smurfs.AddAsync(request.Smurf);
            await unitOfWork.CompleteAsync();

            return Result<Unit>.Success(Unit.Value);
        }
    }
}

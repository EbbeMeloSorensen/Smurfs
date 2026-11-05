using System.Linq.Expressions;
using AutoMapper;
using MediatR;
using Smurfs.Application.Core;
using Smurfs.Domain;
using Smurfs.Persistence;

namespace Smurfs.Application.Smurfs;

public class List
{
    public class Query : IRequest<Result<PagedList<SmurfDto>>>
    {
        public SmurfParams Params { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<PagedList<SmurfDto>>>
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IPagingHandler<SmurfDto> _pagingHandler;

        public Handler(
            IMapper mapper,
            IUnitOfWorkFactory unitOfWorkFactory,
            IPagingHandler<SmurfDto> pagingHandler)
        {
            _mapper = mapper;
            _unitOfWorkFactory = unitOfWorkFactory;
            _pagingHandler = pagingHandler;
        }

        public async Task<Result<PagedList<SmurfDto>>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            var unitOfWork = _unitOfWorkFactory.CreateUnitOfWork();
            var predicates = new List<Expression<Func<Smurf, bool>>>();

            var smurfs = await unitOfWork.Smurfs.FindAsync(predicates);

            var result = _mapper.Map<IEnumerable<SmurfDto>>(smurfs);

            return Result<PagedList<SmurfDto>>.Success(
                _pagingHandler.Create(result, request.Params.PageNumber,
                    request.Params.PageSize)
            );
        }
    }
}

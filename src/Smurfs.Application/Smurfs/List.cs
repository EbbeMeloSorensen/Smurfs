using MediatR;
using Smurfs.Application.Core;

namespace Smurfs.Application.Smurfs;

public class List
{
    public class Query : IRequest<Result<PagedList<SmurfDto>>>
    {
        /// <summary>
        /// Gets the requested one-based page and its size.
        /// </summary>
        public SmurfParams Params { get; init; } = new();
    }

    public class Handler : IRequestHandler<Query, Result<PagedList<SmurfDto>>>
    {
        private readonly ISmurfReadService _readService;

        public Handler(ISmurfReadService readService)
        {
            _readService = readService;
        }

        public async Task<Result<PagedList<SmurfDto>>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            var pageNumber = request.Params.PageNumber;
            var pageSize = request.Params.PageSize;
            if (pageNumber < 1 || pageSize < 1)
                return Result<PagedList<SmurfDto>>.Failure("Page number and page size must be positive.");

            var page = await _readService.GetPageAsync(pageNumber, pageSize, cancellationToken);
            return Result<PagedList<SmurfDto>>.Success(page);
        }
    }
}

using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Smurfs.Application.Core;

namespace Smurfs.Application.Smurfs;

/// <summary>
/// Defines the use case for listing a page of Smurf summaries.
/// </summary>
public class List
{
    /// <summary>
    /// Requests one page of Smurf summaries.
    /// </summary>
    public class Query : IRequest<Result<PagedList<SmurfDto>>>
    {
        /// <summary>
        /// Gets the requested one-based page and its size.
        /// </summary>
        public SmurfParams Params { get; init; } = new();
    }

    /// <summary>
    /// Checks paging inputs and delegates retrieval to the read service.
    /// </summary>
    public class Handler : IRequestHandler<Query, Result<PagedList<SmurfDto>>>
    {
        private readonly ISmurfReadService _readService;

        private readonly ILogger<Handler> _logger;

        /// <summary>
        /// Uses the supplied read service to retrieve pages.
        /// </summary>
        public Handler(ISmurfReadService readService, ILogger<Handler>? logger = null)
        {
            _readService = readService;
            _logger = logger ?? NullLogger<Handler>.Instance;
        }

        /// <summary>
        /// Returns a failure for a nonpositive page number or size.
        /// Otherwise returns the page from the read service and propagates retrieval exceptions.
        /// </summary>
        public async Task<Result<PagedList<SmurfDto>>> Handle(
            Query request,
            CancellationToken cancellationToken)
        {
            var pageNumber = request.Params.PageNumber;
            var pageSize = request.Params.PageSize;
            if (pageNumber < 1 || pageSize < 1)
            {
                _logger.LogDebug("Smurf list rejected invalid paging values. Page {PageNumber}, size {PageSize}.", pageNumber, pageSize);
                return Result<PagedList<SmurfDto>>.Failure("Page number and page size must be positive.");
            }

            var page = await _readService.GetPageAsync(pageNumber, pageSize, cancellationToken);
            _logger.LogInformation("Listed page {PageNumber} of Smurfs with {RowCount} rows.", pageNumber, page.Count);
            return Result<PagedList<SmurfDto>>.Success(page);
        }
    }
}

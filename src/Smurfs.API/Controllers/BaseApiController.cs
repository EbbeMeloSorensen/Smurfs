using MediatR;
using Microsoft.AspNetCore.Mvc;
using Smurfs.API.Extensions;
using Smurfs.Application.Core;

namespace Smurfs.API.Controllers;

/// <summary>
/// Provides shared routing and application result handling for API controllers.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BaseApiController : ControllerBase
{
    private IMediator? _mediator;

    /// <summary>
    /// Gets the MediatR dispatcher for the current request.
    /// </summary>
    protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>();

    /// <summary>
    /// Converts an application result to a success, not-found, or bad-request response.
    /// </summary>
    protected ActionResult HandleResult<T>(Result<T> result) where T : notnull
    {
        if (result == null)
        {
            return NotFound();
        }

        if (result.IsSuccess && result.Value != null)
        {
            return Ok(result.Value);
        }

        if (result.IsSuccess && result.Value == null)
        {
            return NotFound();
        }

        return BadRequest(result.Error);
    }

    /// <summary>
    /// Converts a paged application result to an HTTP response with pagination metadata on success.
    /// </summary>
    protected ActionResult HandlePagedResult<T>(Result<PagedList<T>> result)
    {
        if (result == null)
        {
            return NotFound();
        }

        if (result.IsSuccess && result.Value != null)
        {
            Response.AddPaginationHeader(result.Value.CurrentPage, result.Value.PageSize,
                result.Value.TotalCount, result.Value.TotalPages);
            return Ok(result.Value);
        }

        if (result.IsSuccess && result.Value == null)
        {
            return NotFound();
        }

        return BadRequest(result.Error);
    }
}

using Microsoft.AspNetCore.Mvc;
using Smurfs.Application.Smurfs;

namespace Smurfs.API.Controllers;

/// <summary>
/// Exposes the Smurf catalog over HTTP.
/// </summary>
public class SmurfsController : BaseApiController
{
    /// <summary>
    /// Returns a page of Smurf summaries and its pagination metadata.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult> GetSmurfs(
        [FromQuery] SmurfParams parameters,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new List.Query { Params = parameters }, cancellationToken);
        return HandlePagedResult(result);
    }
}

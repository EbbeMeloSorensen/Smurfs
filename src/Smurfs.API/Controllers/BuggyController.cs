using Microsoft.AspNetCore.Mvc;

namespace Smurfs.API.Controllers;

/// <summary>
/// Provides endpoints for exercising the client's HTTP error handling.
/// </summary>
public class BuggyController : BaseApiController
{
    /// <summary>
    /// Returns a test 404 response.
    /// </summary>
    [HttpGet("not-found")]
    public ActionResult GetNotFound()
    {
        return NotFound();
    }

    /// <summary>
    /// Returns a test 400 response with a message.
    /// </summary>
    [HttpGet("bad-request")]
    public ActionResult GetBadRequest()
    {
        return BadRequest("This is a bad request");
    }

    /// <summary>
    /// Throws a test exception for the server error middleware to handle.
    /// </summary>
    [HttpGet("server-error")]
    public ActionResult GetServerError()
    {
        throw new Exception("This is a server error");
    }

    /// <summary>
    /// Returns a test 401 response.
    /// </summary>
    [HttpGet("unauthorized")]
    public ActionResult GetUnauthorized()
    {
        return Unauthorized();
    }
}

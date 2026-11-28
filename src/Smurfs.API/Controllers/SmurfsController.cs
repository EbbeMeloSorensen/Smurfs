using Smurfs.Domain.VillageLife;
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

    /// <summary>
    /// Returns the fields of one Smurf.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult> GetSmurf(Guid id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new Details.Query { Id = id }, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : NotFound();
    }

    /// <summary>
    /// Creates a Smurf with the proposed name. 
    /// </summary>
    [HttpPost]
    public async Task<ActionResult> CreateSmurf(
        [FromBody] Create.Command command,
        CancellationToken cancellationToken)
    {
        return HandleResult(await Mediator.Send(command, cancellationToken));
    }

    /// <summary>
    /// Replaces the editable details of the Smurf identified by the route.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult> EditSmurf(
        Guid id,
        [FromBody] EditRequest request,
        CancellationToken cancellationToken)
    {
        var command = new Edit.Command
        {
            Id = id,
            Name = request.Name,
            Health = request.Health,
            Birthday = request.Birthday,
            Infected = request.Infected,
            Gender = request.Gender
        };

        return HandleResult(await Mediator.Send(command, cancellationToken));
    }

    /// <summary>
    /// Deletes the Smurf identified by the route.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteSmurf(
        Guid id,
        CancellationToken cancellationToken)
    {
        return HandleResult(await Mediator.Send(new Delete.Command { Id = id }, cancellationToken));
    }

    /// <summary>
    /// Supplies the replacement values for a Smurf edit.
    /// </summary>
    public sealed class EditRequest
    {
        /// <summary>
        /// Gets the proposed name.
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Gets the replacement health.
        /// </summary>
        public required int Health { get; init; }

        /// <summary>
        /// Gets the replacement birthday, or null when unknown.
        /// </summary>
        public DateTime? Birthday { get; init; }

        /// <summary>
        /// Gets replacement infection status, or null when unknown.
        /// </summary>
        public bool? Infected { get; init; }

        /// <summary>
        /// Gets the replacement gender, or null when unknown.
        /// </summary>
        public Gender? Gender { get; init; }

    }
}

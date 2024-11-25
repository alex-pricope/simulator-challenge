using System.Net.Mime;
using Gamebasics.Api.Controllers.v1.Simulation.Dto;
using Gamebasics.Api.Middleware;
using Gamebasics.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Gamebasics.Api.Controllers.v1.Simulation;

[Route("api/v1/simulate")]
[ApiController]
public class SimulationController : ControllerBase
{
    private readonly ISimulationService _simulatorService;

    public SimulationController(ISimulationService simulatorService)
    {
        ArgumentNullException.ThrowIfNull(simulatorService);
        _simulatorService = simulatorService;
    }

    /// <summary>
    /// Run a simulation with 4 teams
    /// </summary>
    /// <param name="groupsSimulationRequest">
    /// The request should have an array of and optional simulation configuration.
    /// </param>
    /// <returns>A simulation result with team summary and rounds</returns>
    /// <remarks>
    /// </remarks>
    /// <response code="201">Returns the simulation result</response>
    /// <response code="400">If request is invalid</response>
    /// <response code="500">Unexpected error</response>
    [HttpPost("groups")]
    [Produces(contentType: MediaTypeNames.Application.Json)]
    [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(GroupSimulationSummaryResponse))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, Type = typeof(ErrorDetails))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, Type = typeof(ErrorDetails))]
    public ActionResult<GroupSimulationSummaryResponse> Post(
        [FromBody] GroupsSimulationCreateRequest groupsSimulationRequest)
    {
        // Get the optional params
        if (groupsSimulationRequest.GoalFactor.HasValue)
            _simulatorService.WithGoalFactor(groupsSimulationRequest.GoalFactor.Value);

        if (groupsSimulationRequest.DrawProbability.HasValue)
            _simulatorService.WithDrawProbability(groupsSimulationRequest.DrawProbability.Value);

        var domainTeams = groupsSimulationRequest.Teams
            .Select(x => x.ToTeamDomain())
            .ToArray();
        
        // Execute the simulation
        var simulationResult = _simulatorService.SimulateGroups(domainTeams);

        // Transform and return
        return Ok(simulationResult.ToGroupsSimulationResult());
    }
}
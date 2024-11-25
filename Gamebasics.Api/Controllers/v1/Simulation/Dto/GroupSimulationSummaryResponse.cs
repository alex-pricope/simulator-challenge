using System.Text.Json.Serialization;

namespace Gamebasics.Api.Controllers.v1.Simulation.Dto;

/// <summary>
/// <para> This type contains the groups simulation results </para>
/// <para> Summary: A summary of all teams ordered by points.
/// See details: <see cref="TeamGroupSummary"/></para>
/// <para>Rounds: The played rounds with teams and scores</para>
/// </summary>
public record GroupSimulationSummaryResponse
{
    [JsonPropertyName("summary")]
    public required Dictionary<string, TeamGroupSummary> Summary { get; set; }

    [JsonPropertyName("rounds")] 
    public required Dictionary<string, List<MatchSummaryLine>> Rounds { get; set; }
}

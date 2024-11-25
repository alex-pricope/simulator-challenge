using System.Text.Json.Serialization;

namespace Gamebasics.Api.Controllers.v1.Simulation.Dto;

/// <summary>
/// A line of a match summary
/// </summary>
public record MatchSummaryLine
{
    /// <summary>
    /// The home team name
    /// </summary>
    [JsonPropertyName("home_team")]
    public required string HomeTeam {get; init;}
    
    /// <summary>
    /// The away team name
    /// </summary>
    [JsonPropertyName("away_team")]
    public required string AwayTeam {get; init;}
    
    /// <summary>
    /// The home team score
    /// </summary>
    [JsonPropertyName("home_score")]
    public required int HomeTeamScore {get; init;}
    
    /// <summary>
    /// The away team score
    /// </summary>
    [JsonPropertyName("away_score")]
    public required int AwayTeamScore {get; init;}

    public override string ToString()
    {
        return $"{HomeTeam} {HomeTeamScore}-{AwayTeamScore} {AwayTeam}";
    }
}
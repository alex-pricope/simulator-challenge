using System.Text.Json.Serialization;

namespace Gamebasics.Api.Controllers.v1.Simulation.Dto;

/// <summary>
/// Summary of a team inside a group
/// </summary>
public class TeamGroupSummary
{
    /// <summary>
    /// Team strength
    /// </summary>\
    [JsonPropertyName("strength")]
    public required int Strength { get; init; }

    /// <summary>
    /// Number of matches played
    /// </summary>
    [JsonPropertyName("played")]
    public required int Played { get; set; }

    /// <summary>
    /// Number of wins
    /// </summary>
    [JsonPropertyName("wins")]
    public required int Wins { get; set; }

    /// <summary>
    /// Number of losses
    /// </summary>
    [JsonPropertyName("losses")]
    public required int Losses { get; set; }

    /// <summary>
    /// Number of draws
    /// </summary>
    [JsonPropertyName("draws")]
    public required int Draws { get; set; }

    /// <summary>
    /// Total number of goals scored by this team
    /// </summary>
    [JsonPropertyName("for")]
    public required int For { get; set; }

    /// <summary>
    /// Total number of goals scored against this team
    /// </summary>
    [JsonPropertyName("against")]
    public required int Against { get; set; }

    /// <summary>
    /// Difference between For and Against
    /// </summary>
    [JsonPropertyName("diff")]
    public required int Diff { get; set; }

    /// <summary>
    /// Total number of points
    /// </summary>
    [JsonPropertyName("points")]
    public required int Points { get; set; }

    public override string ToString()
    {
        return
            $"(Played:{Played} Wins:{Wins} Draws:{Draws} Loss:{Losses} For:{For} Against:{Against} (Diff):{Diff} Points:{Points}";
    }
}
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Gamebasics.Api.Validation;

namespace Gamebasics.Api.Controllers.v1.Simulation.Dto;

public record GroupsSimulationCreateRequest
{
    /// <summary>
    /// <para> The input <see cref="InputTeam"/> array.</para>
    /// The array should be 4 items
    /// </summary>
    [Required(ErrorMessage = "Teams are required. Please provide 4 teams from the existing teams.")]
    [MinLength(4)]
    [MaxLength(4)]
    [JsonPropertyName("teams")]
    [Description("The 4 teams that play in the simulated group.")]
    [TeamNameValidation]
    public required InputTeam[] Teams { get; init; }

    [Description("Custom draw probabilty that will influence the simulation." +
                 "The values should make sense, for example 0.20 = 20%")]
    [DefaultValue(null)]
    [JsonPropertyName("draw_probability")]
    public double? DrawProbability { get; init; }
    
    [Description("Custom goal factor." +
                 "This controls the range of possible scores (win/lose/draw)." +
                 "Directly proportionate to the team strength, acts as a weight")]
    [DefaultValue(null)]
    [JsonPropertyName("goal_factor")]
    public int? GoalFactor { get; init; }
}
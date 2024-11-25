using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Gamebasics.Api.Controllers.v1.Simulation.Dto;

public record InputTeam
{
    [JsonPropertyName("name")]
    [Required(ErrorMessage = "The team name is required", AllowEmptyStrings = false)]
    public required string Name { get; init; }
    
    [JsonPropertyName("strength")]
    [Required(ErrorMessage = "The strength is required")]
    [Range(1, 100, ErrorMessage = "The team strength must be between 1 and 100")]
    public required int Strength { get; init; }
}
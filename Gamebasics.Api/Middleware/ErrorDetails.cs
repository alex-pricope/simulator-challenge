using System.Text.Json;
using System.Text.Json.Serialization;

namespace Gamebasics.Api.Middleware;

/// <summary>
/// This type is returned when any error happens in the API together with the status code
/// </summary>
public record ErrorDetails
{
    /// <summary>
    /// The returned status code
    /// </summary>
    [JsonPropertyName("status")]
    public required int Status { get; init; }
    
    
    /// <summary>
    /// The error details
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; init; }
    
    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
};
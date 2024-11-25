namespace Gamebasics.Domain.Models;

/// <summary>
/// A simulated match result
/// </summary>
public class MatchResult
{
    public required TeamResult TeamAResult { get; init; }
    public required TeamResult TeamBResult { get; init; }
    public bool IsDraw{ get; init; }
}
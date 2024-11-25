namespace Gamebasics.Domain.Models;

/// <summary>
/// Result of matches in a round
/// </summary>
public class RoundResult
{
    public int RoundNumber { get; private set; }
    public List<MatchResult> Matches { get; private set; }

    public RoundResult(int roundNumber)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(roundNumber, 1, nameof(roundNumber));
        RoundNumber = roundNumber;
        Matches = [];
    }

    public void AddMatchResult(MatchResult matchResult)
    {
        ArgumentNullException.ThrowIfNull(matchResult, nameof(matchResult));
        Matches.Add(matchResult);
    }
}
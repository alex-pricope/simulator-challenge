namespace Gamebasics.Domain.Models;

/// <summary>
/// A team result after a simulated match
/// </summary>
public class TeamResult
{
    public Team Team { get; private set; }
    public int Points { get; set; }
    public int GoalsScored { get; set; }
    public int GoalsAllowed { get; set; }
    public bool IsWinner => GoalsScored > GoalsAllowed;

    public TeamResult(Team team)
    {
        ArgumentNullException.ThrowIfNull(team, nameof(team));
        Team = team;
    }
}
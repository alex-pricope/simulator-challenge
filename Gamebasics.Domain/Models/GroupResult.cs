namespace Gamebasics.Domain.Models;

/// <summary>
/// Results of all matches in a group
/// </summary>
public class GroupResult
{
    public string Name{ get; private set; }
    public List<RoundResult> Rounds{ get; private set; }
    
    public GroupResult(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        Name = name;
        Rounds = [];
    }

    public void AddRoundResult(RoundResult roundResult)
    {
        ArgumentNullException.ThrowIfNull(roundResult, nameof(roundResult));
        Rounds.Add(roundResult);
    }
}
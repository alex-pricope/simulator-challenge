namespace Gamebasics.Domain.Models;

/// <summary>
/// The team type.
/// This should be updated with players in the future.
/// The players rankings, play time, etc will influence the Strength
/// I use a simple int as Strength for now to keep this simple
/// </summary>
public class Team
{
    /// <summary>
    /// Team name
    /// </summary>
    public string Name {get; private set;}

    /// <summary>
    /// <para> Team strength. This is a generic value that can be updated depending on other factors.</para>
    /// <para>The value should be: [0 100]</para>
    /// </summary>
    public int Strength {get; private set;}

    public Team(string name, int strength)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        if (strength is <= 0 or > 100)
            throw new ArgumentException("Team strength must be between 1 and 100");
        
        Name = name;
        Strength = strength;
    }

    public override string ToString()
    {
        return $"{Name}({Strength})";
    }
}
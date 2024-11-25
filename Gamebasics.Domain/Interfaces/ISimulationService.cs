using Gamebasics.Domain.Models;

namespace Gamebasics.Domain.Interfaces;

/// <summary>
/// <para>The simulation service</para>
/// <para>This will run a simulation between 2 teams</para>
/// </summary>
public interface ISimulationService
{
    /// <summary>
    /// <para>(Optional) Provide a custom draw probability. This influences how often a match is a draw</para>
    /// <para>Default is 0.25 (25%)</para>
    /// </summary>
    /// <param name="probability">The double value between 0d and 1d: 0.33 = 33%</param>
    ISimulationService WithDrawProbability(double probability);
    
    /// <summary>
    /// <para>(Optional) This controls the range of possible scores (win/lose/draw)</para>
    /// <para>Directly proportionate to the team strength, acts as a weight</para>
    /// <para>Winning team: A lower/higher goal factor, allows a strong team to score high/low.</para>
    /// <para>Losing team: Same as above, this influences the possibility for high/low scores</para>
    /// <para>Default is 30 so that the results seem a bit realistic</para>
    /// <para>Lower value (eg. 10): Encourage high scores for both teams. Results in high-scoring matches</para>
    /// <para>Higher value (eg. 60): Restrict scores to lower ranges. Defensive, low scoring matches</para>
    /// </summary>
    /// <param name="factor">The weight as integer</param>
    ISimulationService WithGoalFactor(int factor);
    
    /// <summary>
    /// Run the simulation and return the <see cref="GroupResult"/>
    /// </summary>
    GroupResult SimulateGroups(Team[] teams);
}
using Gamebasics.Domain.Exceptions;
using Gamebasics.Domain.Interfaces;
using Gamebasics.Domain.Models;
using Microsoft.Extensions.Logging;

namespace Gamebasics.Services;

/// <summary>
/// <inheritdoc cref="ISimulationService"/>
/// </summary>
public class SoccerSimulationService : ISimulationService
{
    private readonly ILogger<SoccerSimulationService> _logger;
    private readonly Random _random = new();
    
    private double _drawProbability = 0.25d;
    private int _goalFactor = 30;

    public SoccerSimulationService(ILogger<SoccerSimulationService> logger)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        _logger = logger;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public GroupResult SimulateGroups(Team[] teams)
    {
        ArgumentOutOfRangeException.ThrowIfZero(teams.Length, nameof(teams.Length));
        if (teams.Length % 2 != 0)
            throw new Exception("Team count must be even");

        // Generate the group rounds
        var simulationRounds = GetGroupRounds(teams.ToList());
        var groupResults = new GroupResult("Match Group");

        // Run the simulations and populate the return data
        try
        {
            foreach (var round in simulationRounds)
            {
                var roundResult = new RoundResult(round.Key);
                foreach (var match in round.Value)
                {
                    var matchResult = RunMatchSimulation(match.Home, match.Away);
                    roundResult.AddMatchResult(matchResult);
                }

                groupResults.AddRoundResult(roundResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in simulation simulation");
            throw new SimulationException(ex.Message);
        }
        
        return groupResults;
    }

    /// <summary>
    /// Using round-robin, get team matches pairing for the groups.
    /// Also it will balance the home/away for the teams so no team is home/away many times 
    /// https://medium.com/coinmonks/sports-scheduling-simplified-the-power-of-the-rotation-algorithm-in-round-robin-tournament-eedfbd3fee8e
    /// </summary>
    /// <param name="teams"><see cref="Team"/></param>
    /// <returns>A dictionary that holds a list of matches for each round</returns>
    internal Dictionary<int, List<(Team Home, Team Away)>> GetGroupRounds(List<Team> teams)
    {
        var rounds = new Dictionary<int, List<(Team Home, Team Away)>>();

        // Number of rounds = teams.Length - 1
        // Matches per round = teams.Length / 2
        var roundCount = teams.Count - 1;
        var matchPerRoundCount = teams.Count / 2;

        // Track home assignments
        var homeCount = teams.ToDictionary(team => team.Name, _ => 0);
        for (var round = 0; round < roundCount; round++)
        {
            var roundPairs = new List<(Team Home, Team Away)>();
            for (var match = 0; match < matchPerRoundCount; match++)
            {
                // First team is from start (increment)
                var teamA = teams[match];

                // Second team is from the end (decrement)
                var teamB = teams[^(match + 1)];

                // We need to balance this so team A is not home in every group
                Team home, away;
                if (homeCount[teamA.Name] <= homeCount[teamB.Name])
                {
                    home = teamA;
                    away = teamB;
                }
                else
                {
                    home = teamB;
                    away = teamA;
                }

                homeCount[home.Name]++;
                roundPairs.Add((home, away));
                _logger.LogInformation("simulator: round {round}: [{home}-{away}]", round + 1, home.Name, away.Name);
            }

            // Add this round's pairs to the rounds dictionary
            rounds.Add(round + 1, roundPairs);

            // Swap second and last inside the array (first one is fixed)
            var last = teams[^1];
            teams.RemoveAt(teams.Count - 1);
            teams.Insert(1, last);
        }

        return rounds;
    }

    internal MatchResult RunMatchSimulation(Team teamA, Team teamB)
    {
        ArgumentNullException.ThrowIfNull(teamA, nameof(teamA));
        ArgumentNullException.ThrowIfNull(teamB, nameof(teamB));
        var isDraw = false;
        int teamAScore = 0, teamBScore = 0;
        var teamAResult = new TeamResult(teamA);
        var teamBResult = new TeamResult(teamB);
        
        // Roll a random double [0d - 1d]
        var outcome = _random.NextDouble();

        // Initial probabilities for each outcome
        var (teamAWinProb, teamBWinProb) = AdjustTeamProbabilities(teamA, teamB);
        
        // The strength ratio is used as a baseline for the max score 
        var teamAStrToGoalFactorRatio = teamA.Strength / _goalFactor;
        var teamBStrToGoalFactorRatio = teamB.Strength / _goalFactor;
        
        // these 2 values are used below to add extra realism to scoring
        const int winGoalBuffer = 2;
        const int loseGoalBuffer = 1;
        
        // if the number is inside [0, A] -> Team A is the winner
        if (outcome < teamAWinProb)
        {
            // Team A wins - roll a number and use it as upper limit for B
            // Add a small constant (+2) to ensure more dynamic scores for the winning team
            // +1 for losing team guarantees the winning team always scores more than the losing one
            teamAScore = _random.Next(1, teamAStrToGoalFactorRatio + winGoalBuffer);
            teamBScore = _random.Next(0, Math.Min(teamAScore, teamBStrToGoalFactorRatio + loseGoalBuffer));

            // Add 3 points to teamA
            teamAResult.Points += 3;
        }
        // if the number is inside [0, (A+B)] -> Team B is the winner
        else if (outcome < teamAWinProb + teamBWinProb)
        {
            // Team B wins - roll a number and use it as upper limit for A
            teamBScore = _random.Next(1, teamBStrToGoalFactorRatio + winGoalBuffer);
            teamAScore = _random.Next(0, Math.Min(teamBScore, teamAStrToGoalFactorRatio + loseGoalBuffer));

            // Add 3 points to teamB
            teamBResult.Points += 3;
        }
        // If outside [0, (A+B)] -> Draw
        else
        {
            // Draw
            // For the draw, the score is influenced by the weaker team strength
            teamAScore = teamBScore =
                _random.Next(0, (Math.Min(teamA.Strength, teamB.Strength) / _goalFactor) + winGoalBuffer);

            // Draw -> 1 point to each team
            teamAResult.Points += 1;
            teamBResult.Points += 1;

            isDraw = true;
        }

        // Update the results
        teamAResult.GoalsScored += teamAScore;
        teamBResult.GoalsScored += teamBScore;

        teamAResult.GoalsAllowed += teamBScore;
        teamBResult.GoalsAllowed += teamAScore;

        _logger.LogInformation("simulator: {homeTeam} vs {awayTeam} - Result: {homeScore}:{awayScore}",
            teamA.Name, teamB.Name, teamAScore, teamBScore);

        return new MatchResult
        {
            TeamAResult = teamAResult,
            TeamBResult = teamBResult,
            IsDraw = isDraw
        };
    }

    private (double TeamAProb, double TeamBProb) AdjustTeamProbabilities(Team teamA, Team teamB)
    {
        double totalStrength = teamA.Strength + teamB.Strength;

        // Initial probabilities for each outcome
        var teamAWinProb = teamA.Strength / totalStrength;
        var teamBWinProb = teamB.Strength / totalStrength;

        // Both team probabilities are reduced proportionally by % of draw probability
        teamAWinProb *= (1 - _drawProbability);
        teamBWinProb *= (1 - _drawProbability);

        return (teamAWinProb, teamBWinProb);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public ISimulationService WithDrawProbability(double drawProbability)
    {
        if (drawProbability is < 0d or > 1d)
            throw new ArgumentOutOfRangeException(nameof(drawProbability),
                "Draw probability must be between 0d and 1d.");

        _drawProbability = drawProbability;
        return this;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public ISimulationService WithGoalFactor(int factor)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(factor, nameof(factor));
        _goalFactor = factor;
        return this;
    }
}
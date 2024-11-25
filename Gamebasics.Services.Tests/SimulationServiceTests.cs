using FluentAssertions;
using Gamebasics.Domain.Models;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Gamebasics.Services.Tests;

[TestFixture]
public class SimulationServiceTests
{
    private SoccerSimulationService _service;
    private Mock<ILogger<SoccerSimulationService>> _loggerMock;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<SoccerSimulationService>>();
        _service = new SoccerSimulationService(_loggerMock.Object);
    }

    [Test]
    public void SimulateGroups_ValidTeams_ReturnsValidGroupResult()
    {
        // Arrange
        var teams = new[]
        {
            new Team("Team A", 80),
            new Team("Team B", 60),
            new Team("Team C", 70),
            new Team("Team D", 50)
        };

        // Act
        var result = _service.SimulateGroups(teams);

        // Assert
        // 4 teams -> 3 rounds, 6 matches
        result.Should().NotBeNull();
        result.Name.Should().Be("Match Group");
        result.Rounds.Should().HaveCount(3);
        result.Rounds.SelectMany(r => r.Matches).Should().HaveCount(6);
    }

    [Test]
    public void SimulateGroups_OddTeamCount_ThrowsException()
    {
        // Arrange
        var teams = new[]
        {
            new Team("Team A", 80),
            new Team("Team B", 60),
            new Team("Team C", 70)
        };

        // Act
        Action action = () => _service.SimulateGroups(teams);

        // Assert
        action.Should().Throw<Exception>()
            .WithMessage("Team count must be even");
    }

    [Test]
    public void GetGroupRounds_NoTeamPlaysTwiceInARound()
    {
        // Arrange
        var teamA = new Team("Team A", 80);
        var teamC = new Team("Team C", 60);
        var teamD = new Team("Team D", 50);
        var teamB = new Team("Team B", 70);

        var teams = new List<Team> { teamB, teamA, teamD, teamC };

        // Act
        var rounds = _service.GetGroupRounds(teams);

        // Assert
        foreach (var round in rounds)
        {
            var participatingTeams = new HashSet<string>();
            foreach (var (home, away) in round.Value)
            {
                participatingTeams.Should().NotContain(home.Name);
                participatingTeams.Should().NotContain(away.Name);

                participatingTeams.Add(home.Name);
                participatingTeams.Add(away.Name);
            }
        }
    }

    [Test]
    public void SimulateGroups_ValidTeams_ProducesValidMatchResults()
    {
        // Arrange
        var teams = new[]
        {
            new Team("Team A", 80),
            new Team("Team B", 60),
            new Team("Team C", 70),
            new Team("Team D", 50)
        };

        // Act
        var result = _service.SimulateGroups(teams);

        // Assert
        foreach (var round in result.Rounds)
        {
            foreach (var match in round.Matches)
            {
                match.TeamAResult.Should().NotBeNull();
                match.TeamBResult.Should().NotBeNull();

                // Check valid points and goals
                match.TeamAResult.Points.Should().BeGreaterThanOrEqualTo(0);
                match.TeamBResult.Points.Should().BeGreaterThanOrEqualTo(0);

                match.TeamAResult.GoalsScored.Should().BeGreaterThanOrEqualTo(0);
                match.TeamBResult.GoalsScored.Should().BeGreaterThanOrEqualTo(0);

                (match.TeamAResult.Points + match.TeamBResult.Points).Should().BeLessOrEqualTo(3);
            }
        }
    }

    [Test]
    public void RunMatchSimulation_ValidTeams_AssignsCorrectPoints()
    {
        // Arrange
        var teamA = new Team("Team A", 80);
        var teamB = new Team("Team B", 60);

        // Act
        var result = _service.RunMatchSimulation(teamA, teamB);

        // Assert
        if (result.IsDraw)
        {
            result.TeamAResult.Points.Should().Be(1);
            result.TeamBResult.Points.Should().Be(1);
        }
        else
        {
            var winner = result.TeamAResult.IsWinner ? result.TeamAResult : result.TeamBResult;
            var loser = result.TeamAResult.IsWinner ? result.TeamBResult : result.TeamAResult;

            winner.Points.Should().Be(3);
            loser.Points.Should().Be(0);
        }
    }

    [Test]
    public void SimulateGroups_ValidTeams_BalancesHomeAndAwayAssignments()
    {
        // Arrange
        var teams = new[]
        {
            new Team("Team A", 80),
            new Team("Team B", 60),
            new Team("Team C", 70),
            new Team("Team D", 50)
        };

        // Act
        var result = _service.SimulateGroups(teams);

        // Assert max 2 home/away matches
        var homeCount = new Dictionary<string, int>();
        var awayCount = new Dictionary<string, int>();

        foreach (var team in teams)
        {
            homeCount[team.Name] = 0;
            awayCount[team.Name] = 0;
        }

        foreach (var round in result.Rounds)
        {
            foreach (var match in round.Matches)
            {
                homeCount[match.TeamAResult.Team.Name]++;
                awayCount[match.TeamBResult.Team.Name]++;
            }
        }

        foreach (var team in teams)
        {
            homeCount[team.Name].Should().BeLessOrEqualTo(2);
            awayCount[team.Name].Should().BeLessOrEqualTo(2);
        }
    }

    [Test]
    public void RunMatchSimulation_DrawProbability_ShouldMatchExpectedRange()
    {
        // Arrange
        var teamA = new Team("Team A", 80);
        var teamB = new Team("Team B", 60);

        // Normal probability
        const double drawProbability = 0.3d;
        _service.WithDrawProbability(drawProbability);

        const int totalMatches = 1000;
        var drawCount = 0;

        // Act
        for (var i = 0; i < totalMatches; i++)
        {
            var result = _service.RunMatchSimulation(teamA, teamB);
            if (result.IsDraw)
            {
                drawCount++;
            }
        }

        // Assert
        var actualDrawRate = (double)drawCount / totalMatches;

        // added some tolerance (+-1) - with 1000 matches the ranges should be OK 
        actualDrawRate.Should().BeInRange(0.2d, 0.4d);
    }

    [Test]
    public void RunMatchSimulation_HighDrawProbability_ShouldMatchExpectedRange()
    {
        // Arrange
        var teamA = new Team("Team A", 80);
        var teamB = new Team("Team B", 60);

        const double drawProbability = 0.8d;
        _service.WithDrawProbability(drawProbability);

        const int totalMatches = 1000;
        var drawCount = 0;

        // Act
        for (var i = 0; i < totalMatches; i++)
        {
            var result = _service.RunMatchSimulation(teamA, teamB);
            if (result.IsDraw)
                drawCount++;
        }

        // Assert
        var actualDrawRate = (double)drawCount / totalMatches;

        // With some tolerance
        actualDrawRate.Should().BeInRange(0.75d, 0.85d);
    }
    
    [Test]
    public void RunMatchSimulation_ZeroDrawProbability_ShouldMatchExpectedRange()
    {
        // Arrange
        var teamA = new Team("Team A", 80);
        var teamB = new Team("Team B", 60);

        const double drawProbability = 0d;
        _service.WithDrawProbability(drawProbability);

        const int totalMatches = 1000;
        var drawCount = 0;

        // Act
        for (var i = 0; i < totalMatches; i++)
        {
            var result = _service.RunMatchSimulation(teamA, teamB);
            if (result.IsDraw)
                drawCount++;
        }

        // Assert
        var actualDrawRate = (double)drawCount / totalMatches;

        // With some tolerance
        actualDrawRate.Should().Be(0d);
    }

    [Test]
    public void GoalFactor_HighFactor_ProducesLowScores_ForBothTeams()
    {
        // Arrange
        var teamA = new Team("Strong Team", 70);
        var teamB = new Team("Weak Team", 30);

        // Set a high goal factor to simulate low-scoring matches
        _service.WithGoalFactor(60);

        const int totalMatches = 1000;
        var scoreFrequencies = new Dictionary<int, int>();

        // Act
        for (var i = 0; i < totalMatches; i++)
        {
            var result = _service.RunMatchSimulation(teamA, teamB);
            if (result.IsDraw) continue;

            var winningScore = result.TeamAResult.IsWinner
                ? result.TeamAResult.GoalsScored
                : result.TeamBResult.GoalsScored;

            if (!scoreFrequencies.ContainsKey(winningScore))
                scoreFrequencies[winningScore] = 0;

            scoreFrequencies[winningScore]++;
        }

        // Assert
        scoreFrequencies.Should().NotBeEmpty();

        var total = scoreFrequencies.Sum(kv => kv.Value);
        var average = scoreFrequencies.Sum(kv => kv.Key * kv.Value) / (double)total;

        LogDistribution(scoreFrequencies);

        // Low average score
        average.Should().BeLessThan(3d);

        // Maximum score should be low
        scoreFrequencies.Keys.Max().Should().BeLessThanOrEqualTo(3);
    }

    [Test]
    public void GoalFactor_LowFactor_ProducesHighScores_ForBothTeams()
    {
        // Arrange
        var teamA = new Team("Strong Team", 70);
        var teamB = new Team("Weak Team", 30);

        // Set a low goal factor to simulate high-scoring matches
        _service.WithGoalFactor(10);

        const int totalMatches = 1000;
        var scoreFrequencies = new Dictionary<int, int>();

        // Act
        for (var i = 0; i < totalMatches; i++)
        {
            var result = _service.RunMatchSimulation(teamA, teamB);
            if (result.IsDraw) continue;

            var winningScore = result.TeamAResult.IsWinner
                ? result.TeamAResult.GoalsScored
                : result.TeamBResult.GoalsScored;

            // Track frequency
            if (!scoreFrequencies.ContainsKey(winningScore))
                scoreFrequencies[winningScore] = 0;

            scoreFrequencies[winningScore]++;
        }

        // Assert
        scoreFrequencies.Should().NotBeEmpty();

        var total = scoreFrequencies.Sum(kv => kv.Value);
        var average = scoreFrequencies.Sum(kv => kv.Key * kv.Value) / (double)total;

        LogDistribution(scoreFrequencies);

        // Average is high
        average.Should().BeGreaterThan(3d);

        // max score should be high 
        scoreFrequencies.Keys.Max().Should().BeGreaterThan(5);
    }

    [Test]
    public void SimulateGroup_StrongerTeams_WinMoreOften()
    {
        // Arrange
        const int totalSimulations = 10000;

        var teamA = new Team("Strong Team A", 80);
        var teamB = new Team("Strong Team B", 70);
        var teamC = new Team("Weak Team C", 50);
        var teamD = new Team("Weak Team D", 40);
        
        var teams = new[] { teamA, teamB, teamC, teamD };

        // track wins
        var (aWin, bWin, cWin, dWin, draw) = (0, 0, 0, 0,0);

        // Act
        for (var i = 0; i < totalSimulations; i++)
        {
            var result = _service.SimulateGroups(teams);

            foreach (var round in result.Rounds)
            {
                foreach (var match in round.Matches)
                {
                    if (match.IsDraw)
                    {
                        draw++;
                        continue;
                    }
                    
                    if (match.TeamAResult.IsWinner)
                    {
                        if (match.TeamAResult.Team == teamA) aWin++;
                        if (match.TeamAResult.Team == teamB) bWin++;
                        if (match.TeamAResult.Team == teamC) cWin++;
                        if (match.TeamAResult.Team == teamD) dWin++;
                    }

                    if (match.TeamBResult.IsWinner)
                    {
                        if (match.TeamBResult.Team == teamA) aWin++;
                        if (match.TeamBResult.Team == teamB) bWin++;
                        if (match.TeamBResult.Team == teamC) cWin++;
                        if (match.TeamBResult.Team == teamD) dWin++;
                    }
                }
            }
        }
        
        Console.WriteLine($"Runs:{totalSimulations * 6} A:{aWin} B:{bWin} C:{cWin} D:{dWin} Draw:{draw}");

        // Assert
        aWin.Should().BeGreaterThan(bWin);
        bWin.Should().BeGreaterThan(cWin);
        cWin.Should().BeGreaterThan(dWin);
    }

    private static void LogDistribution(Dictionary<int, int> distribution)
    {
        var total = distribution.Sum(kv => kv.Value);
        var average = distribution.Sum(kv => kv.Key * kv.Value) / (double)total;

        Console.WriteLine($"average win score: {average}");
        Console.WriteLine("distribution:");
        foreach (var (score, frequency) in distribution.OrderBy(kv => kv.Key))
        {
            Console.WriteLine($"score: {score}, frequency: {frequency}");
        }
    }
}
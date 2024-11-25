using System.Net;
using System.Text.Json;
using FluentAssertions;
using Gamebasics.Api.Controllers.v1.Simulation.Dto;
using Gamebasics.Api.Middleware;
using NUnit.Framework;

namespace Gamebasics.Integration.Tests;

[TestFixture]
public class SimulateApiIntegrationTests : TestingBase
{
    [Test]
    public async Task RunSimulation_WithValidTeams_ReturnsResult() 
    {
        // Arrange
        var teams = new[]
        {
            new InputTeam() { Name = "Team A", Strength = 90 },
            new InputTeam() { Name = "Team B", Strength = 76 },
            new InputTeam() { Name = "Team C", Strength = 40 },
            new InputTeam() { Name = "Team D", Strength = 27 },
        };
        
        var request = new GroupsSimulationCreateRequest
        {
            Teams = teams,
        };

        // Act
        var response = await DoPostRequest(SimulationsRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var simulationResult = JsonSerializer.Deserialize<GroupSimulationSummaryResponse>(content);
        simulationResult.Should().NotBeNull();
        
        // 4 summary line, 3 rounds
        simulationResult.Summary.Should().HaveCount(4);
        simulationResult.Rounds.Should().HaveCount(3);
    }
    
    [Test]
    public async Task RunSimulation_WithDuplicateTeamName_ReturnsBadRequest_WithCorrectCustomError()
    {
        // Arrange
        // No teams in the request
        var teams = new[]
        {
            new InputTeam() { Name = "Team A", Strength = 90 },
            new InputTeam() { Name = "Team B", Strength = 76 },
            new InputTeam() { Name = "Team C", Strength = 40 },
            new InputTeam() { Name = "Team C", Strength = 27 },
        };
        
        var request = new GroupsSimulationCreateRequest
        {
            Teams = teams,
        };

        // Act
        var response = await DoPostRequest(SimulationsRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        
        // Check the custom ErrorDetails
        var errorDetails = JsonSerializer.Deserialize<ErrorDetails>(content);
        errorDetails.Should().NotBeNull();
        errorDetails.Message.Should().Contain("Team name 'Team C' is duplicated");
        errorDetails.Status.Should().Be((int)HttpStatusCode.BadRequest);
    }
    
    [Test]
    public async Task RunSimulation_WithNoTeams_ReturnsBadRequest_WithCorrectCustomError()
    {
        // Arrange
        // No teams in the request
        var request = new GroupsSimulationCreateRequest
        {
            Teams = null,
        };

        // Act
        var response = await DoPostRequest(SimulationsRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        
        // Check the custom ErrorDetails
        var errorDetails = JsonSerializer.Deserialize<ErrorDetails>(content);
        errorDetails.Should().NotBeNull();
        errorDetails.Message.Should().Contain("Teams are required");
        errorDetails.Status.Should().Be((int)HttpStatusCode.BadRequest);
    }
    
    [Test]
    public async Task RunSimulation_WithEmptyTeamName_ReturnsBadRequest_WithCorrectCustomError()
    {
        // Arrange
        var teams = new[]
        {
            new InputTeam() { Name = "", Strength = 90 },
            new InputTeam() { Name = "Team B", Strength = 76 },
            new InputTeam() { Name = "Team C", Strength = 40 },
            new InputTeam() { Name = "Team D", Strength = 27 },
        };
        
        var request = new GroupsSimulationCreateRequest
        {
            Teams = teams,
        };

        // Act
        var response = await DoPostRequest(SimulationsRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        
        // Check the custom ErrorDetails
        var errorDetails = JsonSerializer.Deserialize<ErrorDetails>(content);
        errorDetails.Should().NotBeNull();
        errorDetails.Message.Should().Contain("The team name is required");
        errorDetails.Status.Should().Be((int)HttpStatusCode.BadRequest);
    }
    
    [Test]
    public async Task RunSimulation_WithLessThan4Teams_ReturnsBadRequest_WithCorrectCustomError()
    {
        // Arrange
        // Less than 4 ids
        var request = new GroupsSimulationCreateRequest
        {
            Teams = new[]
            {
                new InputTeam() { Name = "Team A", Strength = 90 },
                new InputTeam() { Name = "Team B", Strength = 76 },
                new InputTeam() { Name = "Team C", Strength = 40 },
            }
        };

        // Act
        var response = await DoPostRequest(SimulationsRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        
        // Check the custom ErrorDetails
        var errorDetails = JsonSerializer.Deserialize<ErrorDetails>(content);
        errorDetails.Should().NotBeNull();
        errorDetails.Message.Should().Contain("minimum length of '4'");
        errorDetails.Status.Should().Be((int)HttpStatusCode.BadRequest);
    }
    
    [Test]
    public async Task RunSimulation_WithDrawProbabilityAndGoalFactor_ReturnResult()
    {
        // Arrange
        var teams = new[]
        {
            new InputTeam() { Name = "Team A", Strength = 90 },
            new InputTeam() { Name = "Team B", Strength = 76 },
            new InputTeam() { Name = "Team C", Strength = 40 },
            new InputTeam() { Name = "Team D", Strength = 27 },
        };

        var request = new GroupsSimulationCreateRequest
        {
            Teams = teams,
            DrawProbability = 0.01d,
            GoalFactor = 10
        };

        // Act
        var response = await DoPostRequest(SimulationsRoute, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var simulationResult = JsonSerializer.Deserialize<GroupSimulationSummaryResponse>(content);
        simulationResult.Should().NotBeNull();

        Console.WriteLine(simulationResult.ToString());

        simulationResult.Summary.Should().HaveCount(4);
        simulationResult.Rounds.Should().HaveCount(3);
    }
}
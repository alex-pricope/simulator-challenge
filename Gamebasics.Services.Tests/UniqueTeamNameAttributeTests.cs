using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Gamebasics.Api.Controllers.v1.Simulation.Dto;
using Gamebasics.Api.Validation;
using NUnit.Framework;

namespace Gamebasics.Services.Tests;

[TestFixture]
public class UniqueTeamNameAttributeTests
{
    private ValidationContext _context;
    
    [SetUp]
    public void Setup()
    {
        _context = new ValidationContext(new object());
    }

    [Test]
    public void Should_ReturnSuccess_When_TeamNamesAreUnique()
    {
        // Arrange
        var teams = new[]
        {
            new InputTeam { Name = "Team A", Strength = 80 },
            new InputTeam { Name = "Team B", Strength = 70 },
            new InputTeam { Name = "Team C", Strength = 60 }
        };

        var attribute = new TeamNameValidation();

        // Act
        var result = attribute.GetValidationResult(teams, _context);

        // Assert
        result.Should().Be(ValidationResult.Success);
    }
    
    [Test]
    public void Should_ReturnValidationError_When_TeamNamesAreDuplicated()
    {
        // Arrange
        var teams = new[]
        {
            new InputTeam { Name = "Team A", Strength = 80 },
            new InputTeam { Name = "Team B", Strength = 70 },
            new InputTeam { Name = "Team A", Strength = 60 }
        };

        var attribute = new TeamNameValidation();

        // Act
        var result = attribute.GetValidationResult(teams, _context);

        // Assert
        result.Should().NotBe(ValidationResult.Success);
        result.ErrorMessage.Should().Be("Team name 'Team A' is duplicated.");
    }
    
    [Test]
    public void Should_ReturnValidationError_When_TeamNameIsNullOrEmpty()
    {
        // Arrange
        var teams = new[]
        {
            new InputTeam { Name = "Team A", Strength = 80 },
            new InputTeam { Name = null, Strength = 70 },
            new InputTeam { Name = "Team B", Strength = 60 }
        };

        var attribute = new TeamNameValidation();

        // Act
        var result = attribute.GetValidationResult(teams, _context);

        // Assert
        result.Should().NotBe(ValidationResult.Success);
        result.ErrorMessage.Should().Be("Found empty/null team name.");
    }
}
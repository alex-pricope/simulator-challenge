using System.ComponentModel.DataAnnotations;
using Gamebasics.Api.Controllers.v1.Simulation.Dto;

namespace Gamebasics.Api.Validation;

public class TeamNameValidation : ValidationAttribute
{
    /// <summary>
    /// <para>Validate a <see cref="InputTeam"/></para>
    /// <para>If the name is empty or null for any item will fail the validation</para>
    /// <para>If the name is not unique inside the collection will fail the validation</para>
    /// </summary>
    /// <param name="value"><see cref="InputTeam"/></param>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not InputTeam[] array) return ValidationResult.Success;
        
        var seenNames = new HashSet<string>();
        foreach (var team in array)
        {
            if (string.IsNullOrWhiteSpace(team.Name))
                return new ValidationResult($"Found empty/null team name.");
            
            if (!seenNames.Add(team.Name))
            {
                return new ValidationResult($"Team name '{team.Name}' is duplicated.");
            }
        }

        return ValidationResult.Success;
    }
} 
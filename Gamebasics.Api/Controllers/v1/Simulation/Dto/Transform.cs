using Gamebasics.Domain.Models;

namespace Gamebasics.Api.Controllers.v1.Simulation.Dto;

public static class Transform
{
    /// <summary>
    /// Transform from a domain object type to the DTO type. 
    /// </summary>
    /// <param name="groupResult"><see cref="GroupResult"/></param>
    /// <returns><see cref="GroupSimulationSummaryResponse"/></returns>
    public static GroupSimulationSummaryResponse ToGroupsSimulationResult(this GroupResult groupResult)
    {
        var teamSummary = new Dictionary<string, TeamGroupSummary>();
        var roundSummary = new Dictionary<string, List<MatchSummaryLine>>();

        foreach (var groupResultRound in groupResult.Rounds)
        {
            // Parse each match
            foreach (var match in groupResultRound.Matches)
            {
                // Add team A
                if (teamSummary.TryGetValue(match.TeamAResult.Team.Name, out var existingTeamASummary))
                    UpdateTeamGroupSummary(existingTeamASummary, match.TeamAResult, match.IsDraw);
                else
                    teamSummary.Add(
                        key: match.TeamAResult.Team.Name,
                        value: NewTeamGroupSummary(match.TeamAResult, match.IsDraw));

                // Add team B
                if (teamSummary.TryGetValue(match.TeamBResult.Team.Name, out var existingTeamBSummary))
                    UpdateTeamGroupSummary(existingTeamBSummary, match.TeamBResult, match.IsDraw);
                else
                    teamSummary.Add(
                        key: match.TeamBResult.Team.Name,
                        value: NewTeamGroupSummary(match.TeamBResult, match.IsDraw));

                if (!roundSummary.ContainsKey(groupResultRound.RoundNumber.ToString()))
                {
                    var roundNumber = groupResultRound.RoundNumber.ToString();
                    roundSummary.Add(roundNumber, [match.ToGameSummaryLine()]);
                    continue;
                }

                roundSummary[groupResultRound.RoundNumber.ToString()].Add(match.ToGameSummaryLine());
            }
        }

        var orderedSummary =  teamSummary
            .OrderByDescending(pair => pair.Value.Points)
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        return new GroupSimulationSummaryResponse()
        {
            Summary = orderedSummary,
            Rounds = roundSummary
        };
    }

    private static void UpdateTeamGroupSummary(TeamGroupSummary existingSummary, TeamResult teamResult, bool isDraw)
    {
        existingSummary.Played++;
        if (teamResult.IsWinner)
            existingSummary.Wins++;
        else
        {
            // Do not record loss if either is not a winner
            if (!isDraw)
                existingSummary.Losses++;
        }

        if (isDraw)
            existingSummary.Draws++;

        existingSummary.For += teamResult.GoalsScored;
        existingSummary.Against += teamResult.GoalsAllowed;
        existingSummary.Diff += teamResult.GoalsScored - teamResult.GoalsAllowed;
        existingSummary.Points += teamResult.Points;
    }
    
    private static TeamGroupSummary NewTeamGroupSummary(TeamResult teamResult, bool isDraw)
    {
        return new TeamGroupSummary()
        {
            Strength = teamResult.Team.Strength,

            Played = 1,
            Wins = teamResult.IsWinner ? 1 : 0,
            Losses = !teamResult.IsWinner && !isDraw ? 1 : 0,
            Draws = isDraw ? 1 : 0,
            For = teamResult.GoalsScored,
            Against = teamResult.GoalsAllowed,
            Diff = teamResult.GoalsScored - teamResult.GoalsAllowed,
            Points = teamResult.Points,
        };
    }
    
    private static MatchSummaryLine ToGameSummaryLine(this MatchResult matchResult)
    {
        return new MatchSummaryLine
        {
            HomeTeam = matchResult.TeamAResult.Team.Name,
            AwayTeam = matchResult.TeamBResult.Team.Name,
            HomeTeamScore = matchResult.TeamAResult.GoalsScored,
            AwayTeamScore = matchResult.TeamBResult.GoalsScored,
        };
    }

    public static Team ToTeamDomain(this InputTeam team)
    {
        return new Team(team.Name, team.Strength);
    }
}
using McRider.Domain.Models;
using System.Text.RegularExpressions;

namespace McRider.Common.Extensions;

public static class TournamentExtensions
{
    public static MatchupEntry? GetEntry(this Player player, Matchup matchup)
    {
        return matchup?.Entries.FirstOrDefault(e => e.Player?.Id == player?.Id);
    }

    public static List<MatchupEntry> GetEntries(this Player player, Tournament tournament)
    {
        return tournament.Rounds.SelectMany(r => r).SelectMany(m => m.Entries).Where(e => e.Player?.Id == player?.Id).ToList();
    }

    public static Tournament FixMatchupRef(this Tournament tournament, Matchup matchup)
    {
        foreach (var round in tournament?.Rounds)
        {
            var indexOf = round.IndexOf(m => m?.Id == matchup?.Id);
            if (indexOf >= 0)
            {
                round[indexOf] = matchup;
                return tournament.FixParentMatchupRef();
            }
        }

        return tournament;
    }

    public static Tournament FixParentMatchupRef(this Tournament tournament)
    {
        var allMatchup = tournament?.Rounds.SelectMany(r => r).ToArray();
        var allEntries = tournament?.Rounds.SelectMany(r => r.SelectMany(m => m.Entries)).ToArray();

        foreach (var m in allMatchup)
        {
            m.Game = tournament.Game;
            
            foreach (var e in allEntries)
            {
                if (e?.ParentMatchup?.Id == m?.Id)
                    e.ParentMatchup = m;                
            }
        }

        return tournament;
    }

    public static Matchup GetNextMatchup(this Tournament tournament, Matchup currentMatchup)
    {
        return tournament?.Rounds.SelectMany(r => r)
            .Where(m => m.IsComplete == false)
            .Where(m => m.Player1 != null && m.Player2 != null)
            .FirstOrDefault(m => m?.Id != currentMatchup?.Id);
    }
    public static double GetPercentageProgress(this MatchupEntry entry, GameItem game, bool? bestOfDistanceVsTime = true)
    {
        if (game is null) return 0;
        if (entry is null) return 0;

        var distanceProgress = game?.TargetDistance <= 0 ? 0 : entry.Distance / game?.TargetDistance;
        var timeProgress = game?.TargetTime?.TotalMicroseconds <= 0 ? 0 : entry.Time?.TotalMicroseconds / game?.TargetTime?.TotalMicroseconds;

        //App.Logger?.LogInformation($"{player.Nickname} Distance progress: {distanceProgress:0.00}, Time progress: {timeProgress:0.00}");

        if (bestOfDistanceVsTime == true)
            return Math.Min(100.0, Math.Max(distanceProgress ?? 0, timeProgress ?? 0) * 100);

        if (bestOfDistanceVsTime == false)
            return Math.Min(100.0, (distanceProgress ?? 0) * 100);

        return Math.Min(100.0, (timeProgress ?? 0) * 100);
    }

    public static double GetPercentageProgress(this Player player, Matchup matchup, bool? bestOfDistanceVsTime = true)
    {
        if (player == null) return 0;
        if (matchup == null) return 0;

        var entry = player.GetEntry(matchup);
        return GetPercentageProgress(entry, matchup.Game, bestOfDistanceVsTime);
    }

    public static double GetPercentageProgress(this Matchup matchup) => GetPlayersProgress(matchup, true).Max();

    public static double GetPercentageTimeProgress(this Matchup matchup) => GetPlayersProgress(matchup, null).Max();

    public static double[] GetPlayersProgress(this Matchup matchup, bool? bestOfDistanceVsTime = true)
    {
        return matchup.Entries.Select(e => GetPercentageProgress(e, matchup.Game, bestOfDistanceVsTime)).ToArray();
    }

    public static void CreateTeamupRounds(this Tournament tournament, IGrouping<string, Player>[] teamsArray)
    {
        // Reset Counter
        Matchup.Counter = 0;
        var matchups = tournament.Rounds.FirstOrDefault() ?? new List<Matchup>();
        matchups.Clear();

        // Schedule game plays so that each player plays once with a player of another team
        for (int i = 0; i < teamsArray.Length; i++)
        {
            for (int j = i + 1; j < teamsArray.Length; j++)  // Ensure pairing between different teams
            {
                var team1Players = teamsArray[i].ToArray();
                var team2Players = teamsArray[j].ToArray();

                foreach (var player1 in team1Players)
                {
                    foreach (var player2 in team2Players)
                    {
                        var entries = new[] { player1, player2 }.Select(p => new MatchupEntry(null, null) { Player = p }).ToList();

                        matchups.Add(new Matchup
                        {
                            Game = tournament.Game,
                            Entries = entries,
                        });
                    }
                }
            }
        }

        // Reset Counter
        Matchup.Counter = 0;
    }
}

using McRider.Domain.Models;

namespace McRider.Common.Extensions;

public static class TournamentExtensions
{
    public static int GetWins(this Player player, Tournament tournament)
        => GetEntries(player, tournament).Count(e => e.CurrentMatchup?.IsByeMatchup == false && e.CurrentMatchup?.HasPlayers == true && e.IsWinner == true);

    public static int GetLooses(this Player player, Tournament tournament)
        => GetEntries(player, tournament).Count(e => e.CurrentMatchup?.IsByeMatchup == false && e.CurrentMatchup?.HasPlayers == true && e.IsWinner == false);

    public static int GetScore(this Player player, Tournament tournament)
        => GetWins(player, tournament);// - GetLooses(player, tournament);

    public static int GetKnockoutRound(this Player player, Tournament tournament)
    {
        var entries = GetEntries(player, tournament);
        var rounds = entries.Select(e => e.CurrentMatchup?.Round ?? 0).Distinct().ToArray();
        return rounds.Max();
    }

    public static Player? GetWinner(this Tournament tournament)
    {
        var finals = tournament.Matchups.Where(m => m.IsFinals()).ToArray();
        var set1 = finals.FirstOrDefault();
        var winnerSet1 = set1?.Winner;

        // Quick exit is set1 has no winner
        if (winnerSet1 is null) return null;

        // Check winner, if they came from winners Brackets then they win tounament
        var winnerSet1Entry = winnerSet1?.GetEntry(set1);
        if (winnerSet1Entry.ParentMatchup?.Bracket == Bracket.Winners)
            return winnerSet1;

        var set2 = finals.LastOrDefault(m => m != set1);
        // Tounament winner is the winner of the last finals set
        return set2?.Winner;
    }

    public static MatchupEntry? GetEntry(this Player player, Matchup matchup)
    {
        return matchup?.Entries.FirstOrDefault(e => e.Player?.Id == player?.Id);
    }

    public static List<MatchupEntry> GetEntries(this Player player, Tournament tournament)
    {
        return tournament.Matchups.SelectMany(m => m.Entries).Where(e => e.Player?.Id == player?.Id).ToList();
    }

    public static Tournament FixMatchupRef(this Tournament tournament, Matchup matchup)
    {
        foreach (var round in tournament.Rounds)
        {
            var indexOf = round.IndexOf(m => m?.Id == matchup?.Id);
            if (indexOf >= 0)
            {
                round[indexOf] = matchup;
                return tournament.FixParentMatchupRef();
            }
        }

        return tournament.FixParentMatchupRef();
    }

    public static Tournament FixParentMatchupRef(this Tournament tournament)
    {
        var allMatchup = tournament.Matchups.ToArray();
        var allEntries = allMatchup.SelectMany(m => m.Entries).ToArray();

        foreach (var m in allMatchup)
        {
            m.Game = tournament.Game;

            var isPlayed = m.Entries.Any(e => e.Distance >= m.Game.TargetDistance || e.Time >= m.Game.TargetTime);
            if (m.IsPlayed == false && isPlayed)
                m.IsPlayed = true;

            foreach (var e in allEntries)
            {
                if (e?.ParentMatchup?.Id == m?.Id && e.ParentMatchup?.Equals(m) != true)
                    e.ParentMatchup = m;
            }
        }

        return tournament;
    }

    public static Matchup? GetNextMatchup(this Tournament tournament, Matchup? currentMatchup = null)
    {
        tournament.FixMatchupRef(currentMatchup);

        // Flatten all matches in all rounds of the tournament
        var readyMatches = tournament.Matchups
            .OrderBy(m => m.Round * (int)m.Bracket)
            .ToArray();

        // Manatory filters
        var manatoryFilters = new Func<Matchup, bool>[]
        {
            // Can't be same player playing themselves
            m => m.Player1?.Nickname != m.Player2?.Nickname,
            // Ignore if the match is already complete
            m => m.IsPlayed != true ,
            // Select matchs where players are assigned
            m => m.HasPlayers == true,
            // Ignore Byes
            m => m.IsByeMatchup == false,
            // Ignore if the match is in the Grand Finals and the set 1 finals is not complete
            m => m.IsFinalsSet2() == false || tournament.RequiresSet2Finals(),
        };

        // Filter conditions to exclude certain matches
        var optionalFilters = new Func<Matchup, bool>[]
        {
            // Ignore the current matchup
            m => currentMatchup == null || m?.Id != currentMatchup?.Id, 
            // Ignore players in the current matchup
            m => currentMatchup == null || m.Players.All(p1 => currentMatchup.Players.All(p2 => p1?.Id != p2?.Id)),
        };

        // Apply Manatory filters
        foreach (var filter in manatoryFilters)
            readyMatches = readyMatches.Where(filter).ToArray();

        // Apply optional filters
        foreach (var filter in optionalFilters)
        {
            var filteredMatches = readyMatches.Where(filter).ToArray();
            // If we still have matches after the filter, use them
            if (filteredMatches.Any())
                readyMatches = filteredMatches;
        }

        // Return the first eligible matchup or null if none found
        return readyMatches.FirstOrDefault();
    }

    public static bool IsFinalsSet1(this Matchup matchup)
    {
        return matchup.IsFinals() && matchup.ParentMatchups.All(p => p.IsFinals()) != true;
    }

    public static bool IsFinalsSet2(this Matchup matchup)
    {
        return matchup.IsFinals() && matchup.ParentMatchups.All(p => p.IsFinals()) == true;
    }

    public static bool IsFinals(this Matchup matchup)
    {
        return matchup?.Bracket == Bracket.GrandFinals;
    }

    public static bool RequiresSet2Finals(this Tournament tournament)
    {
        var finals = tournament.Matchups.Where(m => m.IsFinals()).ToArray();
        var set1 = finals.FirstOrDefault();
        var winnerSet1 = set1?.Winner;

        // Quick exit is set1 has no winner
        if (winnerSet1 is null) return true;

        // Check winner, if they came from winners Brackets then they win tounament
        var winnerSet1Entry = winnerSet1?.GetEntry(set1);
        if (winnerSet1Entry.ParentMatchup?.Bracket == Bracket.Winners)
            return false;

        return true;
    }

    public static double GetPercentageProgress(this MatchupEntry entry, GameItem game, bool? bestOfDistanceVsTime = true)
    {
        if (game is null) return 0;
        if (entry is null) return 0;

        var distanceProgress = game?.TargetDistance <= 0 ? 1 : entry.Distance / game?.TargetDistance;
        var timeProgress = game?.TargetTime?.TotalMicroseconds <= 0 ? 1 : entry.Time?.TotalMicroseconds / game?.TargetTime?.TotalMicroseconds;

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

        var matchups = tournament.Rounds.FirstOrDefault() ?? (tournament.Rounds[0] = new List<Matchup>());
        matchups.Clear();

        for(var i = 0; i < tournament.Players.Count - 1; i++)
        {
            var count = tournament.Players.Count;
            var matchup = new Matchup { Game = tournament.Game };
            var player1 = tournament.Players.ElementAtOrDefault(i);
            var player2 = tournament.Players.ElementAtOrDefault((i + 1) % count);

            if (player1?.Id == player2?.Id) 
                continue;
            matchup.Entries.Add(new MatchupEntry(matchup) { Player = player1 });
            matchup.Entries.Add(new MatchupEntry(matchup) { Player = player2 });

            matchups.Add(matchup);
        }

        // Reset Counter
        Matchup.Counter = 0;
    }
}

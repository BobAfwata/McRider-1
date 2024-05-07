namespace McRider.Common.Extensions;

public static class TournamentExtensions
{
    public static int GetWins(this Player player, Tournament tournament) 
        => GetEntries(player, tournament).Count(e => e.CurrentMatchup?.IsByeMatchup == false && e.CurrentMatchup?.HasPlayers == true && e.IsWinner == true);

    public static int GetLooses(this Player player, Tournament tournament) 
        => GetEntries(player, tournament).Count(e => e.CurrentMatchup?.IsByeMatchup == false && e.CurrentMatchup?.HasPlayers == true && e.IsWinner == false);

    public static int GetScore(this Player player, Tournament tournament)
        => GetWins(player, tournament);// - GetLooses(player, tournament);
        
    public static Player? GetWinner(this Tournament tournament)
    {
        var finals = tournament.Matchups.Where(m => m.IsFinals()).ToArray();
        var set1finals = finals.FirstOrDefault();

        var winnerSet1 = set1finals?.Winner;
        var loserSet1 = set1finals?.Loser;

        if (winnerSet1 is null) return null;
        if (loserSet1 is null || winnerSet1?.Id == loserSet1?.Id)
            return winnerSet1;

        if (loserSet1?.GetLooses(tournament) > winnerSet1?.GetLooses(tournament))
            return winnerSet1;

        return finals.LastOrDefault()?.Winner;

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
        var allMatchup = tournament?.Matchups.ToArray();
        var allEntries = allMatchup.SelectMany(m => m.Entries).ToArray();

        foreach (var m in allMatchup)
        {
            m.Game = tournament.Game;

            if (m.IsComplete == true && m.Entries.Any(e => e.Distance == 0))
                m.IsComplete = true;

            foreach (var e in allEntries)
                if (e?.ParentMatchup?.Id == m?.Id)
                    e.ParentMatchup = m;            
        }

        return tournament;
    }

    public static Matchup? GetNextMatchup(this Tournament tournament, Matchup? currentMatchup = null)
    {
        tournament.FixMatchupRef(currentMatchup);

        // Flatten all matches in all rounds of the tournament
        var readyMatches = tournament?.Matchups.ToArray();

        // Manatory filters
        var manatoryFilters = new Func<Matchup, bool>[]
        {
            // Ignore if the match is already complete
            m => m.IsComplete != false ,
            // Select matchs where players are assigned
            m => m.HasPlayers == true,
            // Ignore Byes
            m => m.IsByeMatchup == false,
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
                        var matchup = new Matchup { Game = tournament.Game };

                        foreach (var player in new[] { player1, player2 })
                        {
                            var entry = new MatchupEntry(matchup) { Player = player };
                            matchup.Entries.Add(entry);
                        }

                        var entries = new[] { player1, player2 }.Select(p => new MatchupEntry(matchup) { Player = p }).ToList();

                        matchups.Add(matchup);
                    }
                }
            }
        }

        // Reset Counter
        Matchup.Counter = 0;
    }
}

using McRider.Domain.Models;

namespace McRider.Common.Extensions;

public static class TournamentDELogicExtensios
{
    public static void CreateTournamentRounds(this Tournament tournament)
    {
        var randomizedPlayers = tournament.Players.OrderBy(x => Guid.NewGuid()).ToList();
        var rounds = FindNumberOfRounds(randomizedPlayers.Count);
        var byes = NumberOfByes(rounds, randomizedPlayers.Count);

        tournament.Rounds.Clear();
        Matchup.Counter = 0;
        CreateWinnersBracket(tournament, rounds, byes, randomizedPlayers);
        CreateLosersBracket(tournament, rounds);
        Matchup.Counter = 0;
    }

    private static void CreateWinnersBracket(Tournament tournament, int rounds, int byes, List<Player> randomizedPlayers)
    {
        int round = 2;
        var previousRound = CreateFirstRound(tournament, byes, randomizedPlayers);
        var currentRound = new List<Matchup>();
        var currentMatchup = new Matchup() { Game = tournament.Game };

        while (round <= rounds)
        {
            foreach (Matchup matchup in previousRound)
            {
                currentMatchup.Entries.Add(new MatchupEntry(matchup, currentMatchup));

                if (currentMatchup.Entries.Count > 1)
                {
                    currentMatchup.Round = round;
                    currentRound.Add(currentMatchup);
                    currentMatchup = new Matchup() { Game = tournament.Game };
                }
            }

            tournament.Rounds.Add(currentRound);
            previousRound = currentRound;

            currentRound = new List<Matchup>();
            round++;
        }
    }

    private static void CreateLosersBracket(Tournament tournament, int rounds)
    {
        int round = 1;
        var previousRound = tournament.Rounds.First();
        var currentRound = new List<Matchup>();
        var currentMatchup = new Matchup() { Round = round, Game = tournament.Game, Bracket = Bracket.Losers };

        while (round < rounds)
        {
            foreach (Matchup matchup in previousRound)
            {
                currentMatchup.Entries.Add(new MatchupEntry(matchup, currentMatchup));

                if (currentMatchup.Entries.Count > 1)
                {
                    currentMatchup.Round = round;
                    currentRound.Add(currentMatchup);
                    currentMatchup = new Matchup() { Round = round, Game = tournament.Game, Bracket = Bracket.Losers };
                }
            }

            tournament.Rounds.ElementAt(round-1).AddRange(currentRound);
            previousRound = currentRound;

            currentRound = new List<Matchup>();
            round++;
        }

        var semiFinals = new[] { tournament.WinnersBracket.LastOrDefault(), tournament.LosersBracket.LastOrDefault() }
            .SelectMany(x => x?.Where(x => x != null) ?? []).ToArray();

        var wbw = tournament.WinnersBracket.LastOrDefault()?.LastOrDefault();
        var lbw = tournament.LosersBracket.LastOrDefault()?.LastOrDefault();

        if (lbw == null) return;

        // Add the final matchup
        var finalMatchup = new Matchup() { Round = round + 1, Game = tournament.Game, Bracket = Bracket.GrandFinals };

        foreach (var matchup in semiFinals)
            finalMatchup.Entries.Add(new MatchupEntry(matchup, lbw));

        if (tournament.Rounds.Count <= round)
            tournament.Rounds.Add([finalMatchup]);
        else
            tournament.Rounds.ElementAt(round - 1).Add(finalMatchup);

    }
    
    private static List<Matchup> CreateFirstRound(Tournament tournament, int byes, List<Player> players)
    {
        HashSet<Matchup> output = new HashSet<Matchup>();
        Matchup matchup = new Matchup() { Round = 1, Game = tournament.Game, Bracket = Bracket.Winners };

        foreach (Player player in players)
        {
            matchup.Entries.Add(new MatchupEntry(matchup, null) { Player = player });

            if (byes > 0 || matchup.Entries.Count > 1)
            {
                matchup.Round = 1;
                output.Add(matchup);
                matchup = new Matchup() { Round = 1, Game = tournament.Game, Bracket = Bracket.Winners };

                if (byes > 0) byes--;
            }
        }

        var list = output.ToList();

        tournament.Rounds.Add(list);

        return list;
    }

    private static int NumberOfByes(int rounds, int numberOfPlayers)
    {
        return (int)Math.Pow(2, rounds) - numberOfPlayers;
    }

    private static int FindNumberOfRounds(int teamCount)
    {
        int output = 1;
        int val = 2;
        while (val < teamCount)
        {
            val *= 2;
            output++;
        }

        return output;
    }

}

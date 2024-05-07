using McRider.Domain.Models;

namespace McRider.Common.Extensions;

public static class TournamentDELogicExtensios
{
    public static void CreateTournamentRounds(this Tournament tournament, bool randomize = true)
    {
        var randomizedPlayers = randomize ? tournament.Players.OrderBy(x => Guid.NewGuid()).ToList() : tournament.Players.ToList();
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
        var currentMatchup = new Matchup() { Game = tournament.Game, Index = 1, Bracket = Bracket.Winners };

        while (round <= rounds)
        {
            currentMatchup.Index = 1;
            foreach (Matchup matchup in previousRound)
            {
                currentMatchup.Entries.Add(new MatchupEntry(currentMatchup, matchup));

                if (currentMatchup.Entries.Count > 1)
                {
                    currentMatchup.Round = round;
                    currentRound.Add(currentMatchup);
                    currentMatchup = new Matchup() { Game = tournament.Game, Index = currentMatchup.Index + 1, Bracket = Bracket.Winners };
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
        var currentMatchup = new Matchup() { Round = round, Index = 1, Game = tournament.Game, Bracket = Bracket.Losers };

        while (round < rounds + 2)
        {
            currentMatchup.Index = 1;
            var match = 0;
            foreach (Matchup matchup in previousRound)
            {
                if (round > 1)
                {
                    var dropMatch = tournament.Rounds.ElementAtOrDefault(round - 1)?.ElementAtOrDefault(match);
                    if (dropMatch != null)
                        currentMatchup.Entries.Add(new MatchupEntry(currentMatchup, dropMatch));
                }

                currentMatchup.Entries.Add(new MatchupEntry(currentMatchup, matchup));
                if (currentMatchup.Entries.Count > 1)
                {
                    currentMatchup.Round = round;
                    currentRound.Add(currentMatchup);
                    currentMatchup = new Matchup() { Round = round, Index = currentMatchup.Index + 1, Game = tournament.Game, Bracket = Bracket.Losers };
                }

                match++;
            }

            if (tournament.Rounds.Count <= round - 1)
                tournament.Rounds.Add([]);

            tournament.Rounds.ElementAt(round - 1).AddRange(currentRound);
            previousRound = currentRound;

            currentRound = new List<Matchup>();
            round++;
        }

        var semiFinals = new[] { tournament.WinnersBracket.LastOrDefault(), tournament.LosersBracket.LastOrDefault() }
            .SelectMany(x => x?.Where(x => x != null) ?? []).ToArray();

        if (semiFinals.Length <= 1) return;

        // Add the final matchup
        var finalMatchup = new Matchup() { Round = round + 1, Index = 1, Game = tournament.Game, Bracket = Bracket.GrandFinals };

        foreach (var matchup in semiFinals)
            finalMatchup.Entries.Add(new MatchupEntry(finalMatchup, matchup));

        if (tournament.Rounds.Count <= round)
            tournament.Rounds.Add([finalMatchup]);
        else
            tournament.Rounds.ElementAt(round - 1).Add(finalMatchup);

        // Create the finals set 2
        var finalMatchup2 = new Matchup() { Round = round + 2, Index = 1, Game = tournament.Game, Bracket = Bracket.GrandFinals };

        foreach (var e in finalMatchup.Entries)
            finalMatchup2.Entries.Add(new MatchupEntry(finalMatchup2, finalMatchup));

        if (tournament.Rounds.Count <= round + 1)
            tournament.Rounds.Add([finalMatchup2]);
        else
            tournament.Rounds.ElementAt(round).Add(finalMatchup2);

    }

    private static List<Matchup> CreateFirstRound(Tournament tournament, int byes, List<Player> players)
    {
        HashSet<Matchup> output = new HashSet<Matchup>();
        Matchup matchup = new Matchup() { Round = 1, Index = 1, Game = tournament.Game, Bracket = Bracket.Winners };

        foreach (Player player in players)
        {
            matchup.Entries.Add(new MatchupEntry(matchup) { Player = player });

            if (byes > 0 || matchup.Entries.Count > 1)
            {
                matchup.Round = 1;
                output.Add(matchup);
                matchup = new Matchup() { Round = 1, Index = matchup.Index + 1, Game = tournament.Game, Bracket = Bracket.Winners };

                if (byes > 0) byes--;
            }
        }

        var list = output.ToList();
        //list.Reverse();

        tournament.Rounds.Add(list);

        return list;
    }

    private static int NumberOfByes(int rounds, int numberOfPlayers)
    {
        return (int)(Math.Pow(2, rounds) - numberOfPlayers);
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

using McRider.Domain.Models;

namespace McRider.Common.Helpers;

public static class TournamentLogic
{
    public static void CreateTeamupRounds(this Tournament tournament, IGrouping<string, Player>[] teamsArray)
    {
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
                        var entries = new[] { player1, player2 }.Select(p => new MatchupEntry { Player = p }).ToList();

                        matchups.Add(new Matchup
                        {
                            Game = tournament.Game,
                            Entries = entries,
                        });
                    }
                }
            }
        }
    }
    public static void CreateTournamentRounds(this Tournament tournament)
    {
        var randomizedPlayers = tournament.Players.OrderBy(x => Guid.NewGuid()).ToList();
        var rounds = FindNumberOfRounds(randomizedPlayers.Count);
        var byes = NumberOfByes(rounds, randomizedPlayers.Count);

        tournament.Rounds.Clear();
        tournament.Rounds.Add(CreateFirstRound(tournament, byes, randomizedPlayers));

        CreateOtherRounds(tournament, rounds);
    }

    private static void CreateOtherRounds(Tournament tournament, int rounds)
    {
        int round = 2;
        List<Matchup> previousRound = tournament.Rounds[0];
        List<Matchup> currentRound = new List<Matchup>();
        Matchup currentMatchup = new Matchup() { Game = tournament.Game };

        while (round <= rounds)
        {
            foreach (Matchup matchup in previousRound)
            {
                currentMatchup.Entries.Add(new MatchupEntry { ParentMatchup = matchup });

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

    private static List<Matchup> CreateFirstRound(Tournament tournament, int byes, List<Player> players)
    {
        List<Matchup> output = new List<Matchup>();
        Player byePlayer = new Player { Id = null, Name = "BYE" };
        Matchup matchup = new Matchup() { Round = 1, Game = tournament.Game };

        foreach (Player player in players)
        {
            matchup.Entries.Add(new MatchupEntry { Player = player });

            if (byes > 0 || matchup.Entries.Count > 1)
            {
                matchup.Round = 1;
                output.Add(matchup);
                matchup = new Matchup() { Round = 1, Game = tournament.Game };

                if (byes > 0) byes--;                
            }

            output.Add(matchup);
        }

        return output;
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

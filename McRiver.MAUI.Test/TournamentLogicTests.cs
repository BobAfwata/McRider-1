using McRider.Common.Extensions;
using McRider.Common.Helpers;
using McRider.Domain.Models;

namespace McRiver.MAUI.Test;

public class TournamentLogicTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        var players = MakeRandomPlayers(3).ToList();
        var tournament = new Tournament()
        {
            Game = new GameItem() { Name = "Game 1" },
            Players = players
        };

        tournament.CreateTournamentRounds();

        for (var i = 0; i <= 2; i++)
            if (tournament.Rounds.Count > i)
                SetRandomScores(tournament.Rounds.ElementAt(i));

        var image = tournament.CreateTournamentImage();

        image.Save("C:\\Users\\nmasuki\\Pictures\\Tournaments\\tournament.png");
    }

    private void SetRandomScores(IEnumerable<Matchup> matchups)
    {
        foreach (var matchup in matchups)
        {
            matchup.IsComplete = true;

            if (matchup.HasNoOpponent && matchup.Round == 1)
                continue;

            var startTime = DateTime.UtcNow.AddSeconds(-Random.Shared.Next(10, 60));
            foreach (var e in matchup.Entries)
            {
                if (e.Player == null) continue;

                e.Distance = matchup.HasNoOpponent ? 1 : Random.Shared.Next(100, 2000);
                e.StartTime = startTime;
                e.LastActivity = startTime.AddSeconds(-Random.Shared.Next(10, 100));
            }
        }
    }

    private IEnumerable<Player> MakeRandomPlayers(int count)
    {
        var index = 0;
        while (index++ < count)
        {
            var firstName = LoremIpsum.GetRandomWord().Capitalize();
            var lastName = LoremIpsum.GetRandomWord().Capitalize();

            yield return new Player
            {
                Name = $"{firstName} {lastName}",
                Email = $"{firstName[0].ToString().ToLower()}{lastName.ToLower()}@xy.com",
                Gender = Random.Shared.Next() > 0.5 ? "M" : "F",
                //Nickname = string.Join("", firstName.Take(3)),
                Nickname = $"P{index}",
                Team = "Team 1"
            };
        }
    }
}
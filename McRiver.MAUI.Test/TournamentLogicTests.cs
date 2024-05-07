using McRider.Common.Extensions;
using McRider.Common.Helpers;
using McRider.Common.Services;
using McRider.Domain.Models;
using System.Drawing;

namespace McRiver.MAUI.Test;

public class TournamentLogicTests
{
    RepositoryService<Tournament> _repository;

    [SetUp]
    public void Setup()
    {
        _repository = new RepositoryService<Tournament>(new FileCacheService());
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

        if (1 == 0)
            for (var i = 0; i < 1; i++)
            {
                if (tournament?.Rounds.Count > i)
                {
                    SetRandomScores(tournament.Rounds.ElementAt(i));
                    var winner = tournament.GetWinner();
                    if (winner != null) break;
                }
            }

        var image = tournament.CreateTournamentImage();

        image.Save("C:\\Users\\nmasuki\\Pictures\\Tournaments\\tournament1.png");
        tournament.Save().Wait();
    }

    [Test]
    public void Test2()
    {
        var tournaments = _repository.Find(t => t.IsPending && t.IsStarted).Result;
        if (tournaments?.Any() != true)
            tournaments = _repository.Find().Result;

        var tournament = tournaments?.LastOrDefault().Save().Result;
        var images = new List<Bitmap>();

        tournament?.CreateTournamentRounds();
        var rounds = tournament?.Rounds.Count;

        images.Add(tournament?.CreateTournamentImage());

        //?.Save($"C:\\Users\\nmasuki\\Pictures\\Tournaments\\tournament.png");

        for (var i = 0; i <= rounds; i++)
        {
            if (tournament?.Rounds.Count > i)
            {
                SetRandomScores(tournament.Rounds.ElementAt(i));
                images.Add(tournament?.CreateTournamentImage());
                var winner = tournament.GetWinner();
                if (winner != null) break;
            }
        }

        GifCreator.CreateGif(images, "C:\\Users\\nmasuki\\Pictures\\Tournaments\\tournament.gif", 10000);

        tournament?.Save();
    }

    [Test]
    public void Test3()
    {
        var tournaments = _repository.Find(t => t.IsPending && t.IsStarted).Result;
        if (tournaments?.Any() != true)
            tournaments = _repository.Find().Result;

        var tournament = tournaments?.FirstOrDefault().Save().Result;

        //var m = tournament.Rounds[1][2];
        //var player = m.Player1;

        if (1 == 2)
        {
            int i = 0;
            //tournament.Players = MakeRandomPlayers(16).ToList();
            //tournament?.CreateTournamentRounds();
            var nextMatchup = tournament?.GetNextMatchup();
            while( i < 5 && nextMatchup != null)
            {
                SetRandomScores(nextMatchup);
                tournament?.CreateTournamentImage()?.Save($"C:\\Users\\nmasuki\\Pictures\\Tournaments\\tournament{++i}.png");
                nextMatchup = tournament?.GetNextMatchup(nextMatchup);
            }
        }

        var s1 = tournament?.Matchups.LastOrDefault(x => x.IsFinalsSet1());
        var s1players = s1.Players.ToArray();

        var s2 = tournament?.Matchups.LastOrDefault(x => x.IsFinalsSet2());
        var s2players = s2.Players.ToArray();

        if (s2players.DistinctBy(p => p?.Id).Count() == 1)
            s2.Entries.ForEach(e => e.Player = null);

        var image = tournament?.CreateTournamentImage(false);

        image?.Save("C:\\Users\\nmasuki\\Pictures\\Tournaments\\tournament.png");

        tournament.Save().Wait();
    }

    private void SetRandomScores(Matchup matchup)
    {
        matchup.IsComplete = true;

        if (matchup.IsByeMatchup == true)
            return;

        var startTime = DateTime.UtcNow.AddSeconds(-Random.Shared.Next(10, 60));
        foreach (var e in matchup.Entries)
        {
            if (e.Player == null) continue;

            e.Distance = Random.Shared.Next(100, 2000);
            e.StartTime = startTime;
            e.LastActivity = startTime.AddSeconds(Random.Shared.Next(10, 100));
        }
    }

    private void SetRandomScores(IEnumerable<Matchup> matchups)
    {
        foreach (var matchup in matchups)
            SetRandomScores(matchup);
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
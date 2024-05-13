using McRider.Common.Extensions;
using McRider.Common.Helpers;
using McRider.Common.Services;
using McRider.Domain.Models;
using System.Drawing;

namespace McRiver.MAUI.Test;

public class TournamentLogicTests
{
    RepositoryService<Tournament> _tournamentRepo;
    RepositoryService<GameItem> _gameRepo;

    [SetUp]
    public void Setup()
    {
        _tournamentRepo = new RepositoryService<Tournament>(new FileCacheService());
        _gameRepo = new RepositoryService<GameItem>(new FileCacheService(), "game-items.json");
    }

    [Test]
    public void Test1()
    {
        var players = MakeRandomPlayers(10).ToList();
        var tournament = new Tournament()
        {
            Game = GetTounamentGame(),
            Players = players
        };

        tournament.CreateTournamentRounds(false);

        if (1 == 1)
        {
            int i = 0;
            //tournament.Players = MakeRandomPlayers(16).ToList();
            //tournament?.CreateTournamentRounds();
            var nextMatchup = tournament?.GetNextMatchup();
            while (i++ < players.Count && nextMatchup != null)
            {
                SetRandomScores(nextMatchup);
                //tournament?.CreateTournamentImage()?.Save($"C:\\Users\\nmasuki\\Pictures\\Tournaments\\tournament{i}.png");
                nextMatchup = tournament?.GetNextMatchup(nextMatchup);
            }
        }

        var image = tournament.CreateTournamentImage(true);

        image.Save("C:\\Users\\nmasuki\\Pictures\\Tournaments\\tournament1.png");
        tournament.Save().Wait();
    }

    [Test]
    public void Test2()
    {
        var tournaments = _tournamentRepo.Find(t => t.IsPending && t.IsStarted).Result;
        if (tournaments?.Any() != true)
            tournaments = _tournamentRepo.Find().Result;

        var tournament = tournaments?.LastOrDefault().Save().Result;
        var images = new List<Bitmap>();

        tournament?.CreateTournamentRounds();
        var rounds = tournament?.Rounds.Count;

        images.Add(tournament?.CreateTournamentImage());

        if (1 == 1)
        {
            int i = 0;
            //tournament.Players = MakeRandomPlayers(16).ToList();
            //tournament?.CreateTournamentRounds();
            var nextMatchup = tournament?.GetNextMatchup();
            while (i++ < 5 && nextMatchup != null)
            {
                SetRandomScores(nextMatchup);
                images.Add(tournament?.CreateTournamentImage());
                nextMatchup = tournament?.GetNextMatchup(nextMatchup);
            }
        }

        GifCreator.CreateGif(images, "C:\\Users\\nmasuki\\Pictures\\Tournaments\\tournament.gif", 10000);

        tournament?.Save();
    }

    [Test]
    public void Test3()
    {
        var tournaments = _tournamentRepo.Find(t => t.IsPending && t.IsStarted).Result;
        if (tournaments?.Any() != true)
            tournaments = _tournamentRepo.Find().Result;

        var tournament = tournaments?.FirstOrDefault().Save().Result;
        var r2m1 = tournament.Rounds[6][0];
        var players = r2m1.Players;

        if (1 == 1)
        {
            int i = 0;
            //tournament.Players = MakeRandomPlayers(16).ToList();
            //tournament?.CreateTournamentRounds();
            var nextMatchup = tournament?.GetNextMatchup();
            while (nextMatchup != null)
            {
                SetRandomScores(nextMatchup);
                //tournament?.CreateTournamentImage()?.Save($"C:\\Users\\nmasuki\\Pictures\\Tournaments\\tournament{i}.png");
                nextMatchup = tournament?.GetNextMatchup(nextMatchup);
            }
        }

        var image = tournament?.CreateTournamentImage();

        image?.Save("C:\\Users\\nmasuki\\Pictures\\Tournaments\\tournament.png");

        tournament.Save().Wait();
    }

    private void SetRandomScores(Matchup matchup)
    {
        matchup.IsPlayed = true;

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

    private GameItem GetTounamentGame()
    {
        return _gameRepo.GetAllAsync().Result.FirstOrDefault(x => x.Name == "Tournamet") ?? new GameItem
        {
            Name = "Tournamet",
            GameType = GameType.Tournament,
            PlayersPerTeam = 16,
            TeamsCount = 1,
            Description = "Description 2",
            TargetDistance = 1000,
            TargetTime = TimeSpan.FromMinutes(5),
            Image = "trophy.png",
        };
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
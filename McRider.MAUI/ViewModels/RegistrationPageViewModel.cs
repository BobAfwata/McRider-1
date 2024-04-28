using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace McRider.MAUI.ViewModels;

public partial class RegistrationPageViewModel : BaseViewModel
{
    [ObservableProperty]
    string _fullName;

    [ObservableProperty]
    string _email;

    [ObservableProperty]
    string _phone;

    [ObservableProperty]
    string _nickname;

    [ObservableProperty]
    ObservableCollection<Player> _players = [];

    GameItem _game;

    [RelayCommand]
    private async Task Next()
    {
        if (!IsValidPlayer())
        {
            _logger?.LogWarning("Invalid player data");
            return;
        }

        var player = new Player
        {
            Name = FullName,
            Email = Email,
            PhoneNumber = Phone,
            Nickname = Nickname
        };

        Players.Add(player);
        _tcs?.SetResult(player);

        // Close after all players are registered
        if (Players.Count >= _game?.TeamsCount * _game?.PlayersPerTeam)
            await StartGamePage.GetGamePlays(Players.ToArray(), _game);
    }

    private bool IsValidPlayer()
    {
        return !string.IsNullOrWhiteSpace(FullName) && !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Nickname);
    }

    TaskCompletionSource<Player> _tcs;
    public async Task<List<Player>> AwaitPlayersFor(GameItem game)
    {
        _game = game;

        var players = new List<Player>();
        game.TeamsCount = Math.Max(1, game.TeamsCount);
        game.PlayersPerTeam = Math.Max(1, game.PlayersPerTeam);

        //await Shell.Current.Navigation.PushAsync(new RegistrationPage(this));
        await Shell.Current.GoToAsync($"{nameof(RegistrationPage)}");

        for (var i = 0; i < game.TeamsCount; i++)
        {
            for (var j = 0; j < game.PlayersPerTeam; j++)
            {
                ResetInput((i + 1) * (j + 1));
                Title = game.TeamsCount > 1 ? $"Team {i + 1}, Player {j + 1}" : $"Player {(i + 1) * (j + 1)}";

                _tcs = new TaskCompletionSource<Player>();
                var player = await _tcs.Task;
                
                player.Team = game.TeamsCount > 1 ? $"Team {i + 1}" : "Single Player";

                if (player != null) players.Add(player);
            }
        }

        return players;
    }

    private void ResetInput(int index = 0)
    {
        // Reset input fields
        FullName = $"Player {index}";
        Email = "";
        Phone = "";
        Nickname = $"P{index}";
    }
}

public class Player
{
    private double _distance;

    public string? Id { get; set; } = Guid.NewGuid().ToString();

    public string Name { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Nickname { get; set; }
    public string Team { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime? StartTime { get; set; }
    public DateTime? LastActivity { get; set; }

    public TimeSpan? Time
    {
        get
        {
            if (StartTime.HasValue == false) return null;
            return LastActivity - StartTime;
        }
    }
    public double Distance
    {
        get => _distance;
        set
        {
            var delta = Math.Abs(_distance - value);
            if (delta <= 0.001) return;

            if (StartTime.HasValue == false)
                StartTime = DateTime.UtcNow;

            LastActivity = DateTime.UtcNow;
            _distance = value;
        }
    }

    public TimeSpan? BestTime { get; set; }
    public bool IsWinner { get; internal set; }

    public void Reset()
    {
        _distance = 0;
        StartTime = null;
        LastActivity = null;
    }
}

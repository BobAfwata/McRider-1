
namespace McRider.MAUI.ViewModels;

public partial class RegistrationPageViewModel : BaseViewModel
{
    GameItem _game;

    [ObservableProperty]
    string _fullName = "";

    [ObservableProperty]
    string _email;

    [ObservableProperty]
    string _phone;

    [ObservableProperty]
    string _nickname;

    [ObservableProperty]
    string _gender;

    [ObservableProperty]
    ObservableCollection<Player> _players = [];

    public bool CanSkip => Players.Count >= 3;
    public bool IsMale => _gender?.FirstOrDefault() == 'M';
    public bool IsFemale => _gender?.FirstOrDefault() == 'F';

    public bool IsValidFullName => !string.IsNullOrWhiteSpace(FullName);
    public bool IsValidEmail => !string.IsNullOrWhiteSpace(Email) && Email?.IsEmail() == true;
    public bool IsValidPhone => string.IsNullOrWhiteSpace(Phone) || Regex.IsMatch(Phone, @"07\d{");
    public bool IsValidNickname => !string.IsNullOrWhiteSpace(Nickname);
    public bool IsValidGender => IsMale || IsFemale;

    public bool IsValidated { get; set; }
    public bool IsNotValidated => !IsValidated;
    public bool IsValid => IsValidFullName && IsValidEmail && IsValidNickname && IsValidGender;

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName?.StartsWith("Is") != true)
        {
            if (e.PropertyName != nameof(CanSkip))
                OnPropertyChanged(nameof(CanSkip));

            OnPropertyChanged(nameof(IsValidFullName));
            OnPropertyChanged(nameof(IsValidEmail));
            OnPropertyChanged(nameof(IsValidNickname));
            OnPropertyChanged(nameof(IsValidGender));
            OnPropertyChanged(nameof(IsValid));
            OnPropertyChanged(nameof(IsValidated));
            OnPropertyChanged(nameof(IsNotValidated));
        }

        base.OnPropertyChanged(e);
    }

    private bool IsValidPlayer()
    {
        IsValidated = true;

        return IsValid;
    }

    partial void OnGenderChanged(string value)
    {
        OnPropertyChanged(nameof(IsMale));
        OnPropertyChanged(nameof(IsFemale));
    }

    [RelayCommand]
    private void GenderClicked(object arg)
    {
        if (arg is string gender)
            Gender = gender;
    }

    [RelayCommand]
    private async Task StartGame()
    {
        await Shell.Current.GoToAsync($"///{nameof(StartGamePage)}");
        var vm = App.ServiceProvider.GetService<StartGamePageViewModel>();

        if (vm is not null)
        {
            var tournament = await vm.AwaitMatchupsFor(Players.ToArray(), _game);
            await tournament.Save();
        }
    }

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
            Nickname = Nickname,
            Gender = Gender
        };

        Players.Add(player);
        _tcs?.SetResult(player);

        // Start Game after all players are registered
        if (Players.Count >= _game?.TeamsCount * _game?.PlayersPerTeam)
            await StartGame();
        else
        {
            // Animate to the left
            //await Content.TranslateTo(-Content.Width, 0, 1000, Easing.SinInOut);
        }
    }


    TaskCompletionSource<Player> _tcs;
    public async Task<List<Player>> AwaitPlayersFor(GameItem game)
    {
        _game = game;

        var players = new List<Player>();
        game.TeamsCount = Math.Max(1, game.TeamsCount);
        game.PlayersPerTeam = Math.Max(1, game.PlayersPerTeam);

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
        Gender = "";
        Nickname = $"P{index}";
    }
}


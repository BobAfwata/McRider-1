
namespace McRider.MAUI.ViewModels;

public partial class MatchupPageViewModel : BaseViewModel
{
    private ArdrinoCommunicator _communicator;

    [ObservableProperty]
    Matchup _matchup;

    [ObservableProperty]
    int _countDown;

    public bool ShowCountDown => _countDown > 0;
    partial void OnCountDownChanging(int oldValue, int newValue)
    {
        OnPropertyChanged(nameof(ShowCountDown));
        if (newValue == 0)
            _ = StartGame();
    }

    public MatchupPageViewModel(ArdrinoCommunicator communicator)
    {
        Title = "Game Play";
        _communicator = communicator;
    }

    public bool IsPlayer1Winner => Matchup?.GetWinner()?.Id == Matchup?.Player1?.Id && Matchup?.Player1?.Id != null;
    public bool IsPlayer2Winner => Matchup?.GetWinner()?.Id == Matchup?.Player2?.Id && Matchup?.Player2?.Id != null;
    public double PercentageTimeProgress => Matchup?.GetPercentageTimeProgress() ?? 0;
    public double Player1Progress => Matchup?.GetPlayersProgress().ElementAtOrDefault(0) ?? 0;
    public double Player2Progress => Matchup?.GetPlayersProgress().ElementAtOrDefault(1) ?? 0;
    public double Player1BottleProgress => (Matchup?.GetPlayersProgress().ElementAtOrDefault(0) ?? 0) * 580.0 / 100.0;
    public double Player2BottleProgress => (Matchup?.GetPlayersProgress().ElementAtOrDefault(1) ?? 0) * 580.0 / 100.0;

    private async Task StartCountDown(int countDown = 3)
    {
        CountDown = countDown;
        if (countDown <= 0)
            await StartGame();
        else
        {
            App.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                var countDown = CountDown - 1;

                if (countDown < 0)
                    _ = StartGame().ContinueWith(t => CountDown = countDown);
                else
                    CountDown = countDown;

                return countDown > 0;
            });
        }
    }

    [RelayCommand]
    private async Task StartGame()
    {
        // Stop the game
        await _communicator.Start(Matchup);
    }

    [RelayCommand]
    private async Task StopGame()
    {
        // Stop the game
        await _communicator.Stop();
    }

    TaskCompletionSource<Matchup> _tcs;

    public async Task<Matchup> StartMatchup(Matchup matchup)
    {
        Matchup = matchup;
        _tcs = new TaskCompletionSource<Matchup>();

        if ((await _communicator.Initialize() != true))
        {
            _tcs.SetException(new InvalidOperationException("Failed to initialize the game play! Check configs"));
            return await _tcs.Task;
        }

        // Check every second for game updates untill we have a winner
        App.StartTimer(TimeSpan.FromSeconds(1), () =>
        {
            OnPropertyChanged(nameof(IsPlayer1Winner));
            OnPropertyChanged(nameof(IsPlayer2Winner));
            OnPropertyChanged(nameof(PercentageTimeProgress));
            OnPropertyChanged(nameof(Player1Progress));
            OnPropertyChanged(nameof(Player2Progress));
            OnPropertyChanged(nameof(Player1BottleProgress));
            OnPropertyChanged(nameof(Player2BottleProgress));

            // Return true to continue the timer, false to stop it
            return Matchup.GetWinner() == null;
        });

        _communicator.OnPlayerDisconnected += async (sender, player) =>
        {
            // TODO: Player disconnect notification
        };

        _communicator.OnPlayerStopped += async (sender, player) =>
        {
            // TODO: Player stopped notification
        };

        _communicator.OnPlayerStart += async (sender, player) =>
        {
            var entry = player.GetEntry(Matchup);
            if (entry != null)
                entry.StartTime ??= DateTime.UtcNow;
        };

        _communicator.OnPlayerWon += async (sender, player) =>
        {
            var entry = player.GetEntry(Matchup);
            if (entry != null)
                entry.IsWinner = true;
        };

        _communicator.OnMatchupFinished += async (sender, game) =>
        {
            // Close the game play page
            await StopGame();

            // Give the winner some time to celebrate
            await Task.Delay(TimeSpan.FromSeconds(20));

            // Return the game play
            _tcs.TrySetResult(game);
        };

        await StartCountDown(3);

        return await _tcs.Task;
    }
}
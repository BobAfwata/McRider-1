
using McRider.Domain.Models;
using System.Drawing;

namespace McRider.MAUI.ViewModels;

public partial class MatchupPageViewModel : BaseViewModel
{
    RepositoryService<Tournament> _repository;
    private ArdrinoCommunicator _communicator;

    [ObservableProperty]
    Matchup _matchup;

    [ObservableProperty]
    int _countDown;

    [ObservableProperty]
    bool _showCountDown;


    [ObservableProperty]
    public bool _showResults;

    partial void OnCountDownChanging(int oldValue, int newValue)
    {
        OnPropertyChanged(nameof(ShowCountDown));
    }

    public MatchupPageViewModel(ArdrinoCommunicator communicator, RepositoryService<Tournament> repository)
    {
        Title = "Game Play";
        Tournament = null;
        _communicator = communicator;
        _repository = repository;
    }

    public Tournament Tournament { get; set; }

    public ImageSource TournamentImageSource
    {
        get
        {
            if (Tournament?.Game?.GameType != GameType.Tournament)
                return null;

            var _tournamentImage = Tournament.CreateTournamentImage();
            if (_tournamentImage is not null)
                return ImageSource.FromStream(() => _tournamentImage.ToStream());

            return null;
        }
    }

    public bool IsComplete => IsPlayer1Winner || IsPlayer2Winner;
    public bool IsPlayer1Winner => Matchup?.Winner?.Id == Matchup?.Player1?.Id && Matchup?.Player1?.Id != null;
    public bool IsPlayer2Winner => Matchup?.Winner?.Id == Matchup?.Player2?.Id && Matchup?.Player2?.Id != null;
    public double PercentageTimeProgress => Matchup?.GetPercentageTimeProgress() ?? 0;
    public double Player1Progress => Matchup?.GetPlayersProgress().ElementAtOrDefault(0) ?? 0;
    public double Player2Progress => Matchup?.GetPlayersProgress().ElementAtOrDefault(1) ?? 0;
    public double Player1BottleProgress => (Matchup?.GetPlayersProgress().ElementAtOrDefault(0) ?? 0) * 580.0 / 100.0;
    public double Player2BottleProgress => (Matchup?.GetPlayersProgress().ElementAtOrDefault(1) ?? 0) * 580.0 / 100.0;

    public Matchup NextMatch => Tournament?.GetNextMatchup(Matchup);
    public MatchupEntry WinningEntry => Matchup?.Winner?.GetEntry(Matchup);
    public MatchupEntry Player1Entry => Matchup?.Player1?.GetEntry(Matchup);
    public MatchupEntry Player2Entry => Matchup?.Player2?.GetEntry(Matchup);

    partial void OnShowResultsChanged(bool value)
    {
        if (value == true)
            OnPropertyChanged(nameof(TournamentImageSource));
    }

    partial void OnMatchupChanged(Matchup value)
    {
        OnPropertyChanged(nameof(NextMatch));
        OnPropertyChanged(nameof(IsComplete));
        OnPropertyChanged(nameof(WinningEntry));
        OnPropertyChanged(nameof(Player1Entry));
        OnPropertyChanged(nameof(Player2Entry));
        OnPropertyChanged(nameof(IsPlayer1Winner));
        OnPropertyChanged(nameof(IsPlayer2Winner));
        OnPropertyChanged(nameof(PercentageTimeProgress));
        OnPropertyChanged(nameof(Player1Progress));
        OnPropertyChanged(nameof(Player2Progress));
        OnPropertyChanged(nameof(Player1BottleProgress));
        OnPropertyChanged(nameof(Player2BottleProgress));
    }

    private async Task StartCountDown(int countDown = 3)
    {
        CountDown = countDown;
        if (countDown > 0)
            ShowCountDown = true;

        if (countDown <= 0)
            await StartGame();
        else
            Device.StartTimer(TimeSpan.FromSeconds(1), DoCountDown);
    }

    private bool DoCountDown()
    {
        var countDown = CountDown - 1;

        if (countDown <= 0)
            _ = StartGame();

        if (countDown < 0)
            ShowCountDown = false;

        if (countDown >= 0)
            CountDown = countDown;

        return countDown > 0;
    }

    private async Task StartNextGame()
    {
        await Shell.Current.GoToAsync($"///{nameof(StartGamePage)}");
        var vm = App.ServiceProvider.GetService<StartGamePageViewModel>();

        if (vm is not null)
        {
            var tournament = await vm.AwaitMatchupsFor(Tournament, Tournament.Game);
            await tournament.Save();
        }
        else
        {
            _logger.LogError(CountDown > 0 ? "Failed to start the game" : "Game has ended but could not start another!");
        }
    }

    [RelayCommand]
    private async Task StartGame()
    {
        ShowResults = false;
        // Stop the game
        await _communicator.Start(Matchup);
    }

    [RelayCommand]
    private async Task StopGame()
    {
        // Stop the game
        await _communicator.Stop();
    }

    [RelayCommand]
    private async Task Next()
    {
        ShowResults = false;
        if (NextMatch != null)
        {
            _ = StartNextGame();
        }
        else
        {
            _tcs.TrySetResult(Matchup);
            
            var page = await Shell.Current.Navigation.PopAsync();
            while(page != null) page = await Shell.Current.Navigation.PopAsync();

            await Shell.Current.GoToAsync($"//{nameof(LandingPage)}");
        }
    }

    TaskCompletionSource<Matchup> _tcs;
    public async Task<Matchup> StartMatchup(Matchup matchup, int countDown = 3)
    {
        Tournament = Tournament ?? (await _repository.Find(t => t.Rounds.Any(r => r.Any(m => m.Id == matchup.Id)))).FirstOrDefault();
        Matchup = Tournament.FixParentMatchupRef()?.Matchups.FirstOrDefault(m => m?.Id == matchup?.Id);

        _tcs = new TaskCompletionSource<Matchup>();
        IsBusy = true;
        if ((await _communicator.Initialize() != true))
        {
            IsBusy = false;
            _tcs.SetException(new InvalidOperationException("Failed to initialize the game play! Check configs"));
            return await _tcs.Task;
        }

        IsBusy = false;

        // Check every second for game updates untill we have a winner
        Device.StartTimer(TimeSpan.FromMicroseconds(500), () =>
        {
            try
            {
                // Update the game play
                OnMatchupChanged(Matchup);
            }
            catch (Exception e)
            {

            }
            // Return true to continue the timer, false to stop it
            return Matchup?.IsPlayed != true;
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
            if (entry is not null)
            {
                // Hide count down text
                Device.StartTimer(TimeSpan.FromSeconds(3), () => ShowCountDown = false);
                entry.StartTime ??= DateTime.UtcNow;
            }
        };

        _communicator.OnPlayerWon += async (sender, player) =>
        {
            var entry = player.GetEntry(Matchup);
            //if (entry is not null)
            //    entry.IsWinner = true;
        };

        _communicator.OnMatchupFinished += async (sender, matchup) =>
        {
            Tournament?.FixMatchupRef(matchup);

            // Save the game play
            await Tournament?.Save();

            // Close the game play page
            await StopGame();

            // Give the winner some time to celebrate
            await Task.Delay(TimeSpan.FromSeconds(5));

            // 
            ShowResults = true;

            // Return the game play
            //_tcs.TrySetResult(matchup);
        };

        await StartCountDown(countDown);

        return await _tcs.Task;
    }
}
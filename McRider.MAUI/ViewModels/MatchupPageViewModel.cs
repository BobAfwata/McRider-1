
using McRider.Domain.Models;
using McRider.MAUI.Messages;
using System.Drawing;

namespace McRider.MAUI.ViewModels;

public partial class MatchupPageViewModel : BaseViewModel
{
    RepositoryService<Tournament> _repository;
    private ArdrinoCommunicator _communicator;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsComplete))]
    [NotifyPropertyChangedFor(nameof(IsSinglePlayer))]
    [NotifyPropertyChangedFor(nameof(IsMultiplePlayers))]
    [NotifyPropertyChangedFor(nameof(IsPlayer1Winner))]
    [NotifyPropertyChangedFor(nameof(IsPlayer2Winner))]
    [NotifyPropertyChangedFor(nameof(PercentageTimeProgress))]
    [NotifyPropertyChangedFor(nameof(Player1Progress))]
    [NotifyPropertyChangedFor(nameof(Player2Progress))]
    [NotifyPropertyChangedFor(nameof(Player1ProgressFillHeight))]
    [NotifyPropertyChangedFor(nameof(Player2ProgressFillHeight))]
    [NotifyPropertyChangedFor(nameof(NextMatch))]
    [NotifyPropertyChangedFor(nameof(WinningEntry))]
    [NotifyPropertyChangedFor(nameof(Player1Entry))]
    [NotifyPropertyChangedFor(nameof(Player2Entry))]
    [NotifyPropertyChangedFor(nameof(RevealImage))]
    [NotifyPropertyChangedFor(nameof(PromoImage))]
    Matchup _matchup;

    [ObservableProperty]
    Tournament _tournament;

    [ObservableProperty]
    int _countDown;

    [ObservableProperty]
    bool _showCountDown;

    [ObservableProperty]
    bool _showResults;

    [ObservableProperty]
    double _curtainWidth = 500;

    [ObservableProperty]
    double _progressFillHeight = 580;

    double _player1CurtainCounter = 0;
    double _player2CurtainCounter = 0;

    public MatchupPageViewModel(ArdrinoCommunicator communicator, RepositoryService<Tournament> repository)
    {
        Title = "Game Play";

        _tournament = null;
        _communicator = communicator;
        _repository = repository;

        App.StartTimer(TimeSpan.FromSeconds(1), () =>
        {
            if (IsComplete) return false;

            if (Player1CurtainTranslationX < 0)
                _player1CurtainCounter--;

            if (Player2CurtainTranslationX > 0)
                _player2CurtainCounter--;

            return true;
        });
    }

    public ImageSource? RevealImage => Matchup?.Metadata?.TryGetValue("RevealImage", out var obj) == true && obj is string ? obj?.ToString().ToImageSource() : null;
    
    public ImageSource PromoImage
    {
        get
        {
            var assembly = Application.Current?.GetType().Assembly;
            var revealRegex = new Regex($"{App.Configs?.Theme ?? ""}(.?)promo(.*).png");
            var matches = assembly?.GetManifestResourceNames().Where(r => revealRegex.IsMatch(r));

            return matches?.FirstRandom()?.ToImageSource();
        }
    }

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

    public bool IsSinglePlayer => IsMultiplePlayers != true;
    public bool IsMultiplePlayers => Matchup?.Players.DistinctBy(p => p?.Nickname).Count() > 1;
    public bool IsComplete => Matchup?.IsPlayed == true;
    public bool IsPlayer1Winner => Matchup?.IsPlayed == true && Matchup?.Winner?.Id == Matchup?.Player1?.Id && Matchup?.Player1?.Id != null;
    public bool IsPlayer2Winner => Matchup?.IsPlayed == true && Matchup?.Winner?.Id == Matchup?.Player2?.Id && Matchup?.Player2?.Id != null;

    public double PercentageTimeProgress => Matchup?.GetPercentageTimeProgress() ?? 0;
    public double Player1Progress => Matchup?.GetPlayersProgress(false).ElementAtOrDefault(0) ?? 0;
    public double Player2Progress => Matchup?.GetPlayersProgress(false).ElementAtOrDefault(1) ?? 0;

    public double Player1ProgressFillHeight => Matchup == null ? 0 : Player1Progress / 100.0 * ProgressFillHeight;
    public double Player2ProgressFillHeight => Matchup == null ? 0 : Player2Progress / 100.0 * ProgressFillHeight;

    public double Player1CurtainTranslationX => Matchup == null ? 0 : -(Player1Progress / 100.0 * CurtainWidth / 2 - _player1CurtainCounter);
    public double Player2CurtainTranslationX => Matchup == null ? 0 : IsSinglePlayer ? -Player1CurtainTranslationX : (Player2Progress / 100.0 * CurtainWidth / 2 - _player2CurtainCounter);

    public Matchup NextMatch => Tournament?.GetNextMatchup(Matchup);
    public MatchupEntry WinningEntry => Matchup?.Winner?.GetEntry(Matchup);
    public MatchupEntry Player1Entry => Matchup?.Player1?.GetEntry(Matchup);
    public MatchupEntry Player2Entry => Matchup?.Player2?.GetEntry(Matchup);

    partial void OnShowResultsChanged(bool value)
    {
        if (value == true)
            OnPropertyChanged(nameof(TournamentImageSource));
    }

    private async Task StartCountDown(int countDown = 3)
    {
        CountDown = countDown;
        if (countDown > 0)
            ShowCountDown = true;

        if (countDown <= 0)
            await StartGame();
        else
            Device.StartTimer(TimeSpan.FromSeconds(1.2), () => DoCountDown().Result);
    }

    private async Task<bool> DoCountDown()
    {
        var countDown = CountDown - 1;

        if (countDown < 0)
            ShowCountDown = false;

        if (countDown >= 0)
            CountDown = countDown;

        if (countDown <= 0)
            await StartGame();

        return countDown > 0;
    }

    private bool RefreshProgressView()
    {
        _logger.LogInformation("Matchup progress changed. Progress {Progress:0.00}%", Matchup?.GetPercentageProgress());

        //Update the game play progress
        OnPropertyChanged(nameof(IsComplete));
        OnPropertyChanged(nameof(Player1CurtainTranslationX));
        OnPropertyChanged(nameof(Player2CurtainTranslationX));
        OnPropertyChanged(nameof(Player1ProgressFillHeight));
        OnPropertyChanged(nameof(Player2ProgressFillHeight));
        OnPropertyChanged(nameof(PercentageTimeProgress));
        OnPropertyChanged(nameof(Player1Progress));
        OnPropertyChanged(nameof(Player2Progress));

        // Broudcast the game play progress
        WeakReferenceMessenger.Default.Send(new TournamentProgress(Tournament, Matchup, Matchup?.GetPercentageProgress() ?? 0));

        // Return true to continue the timer, false to stop it
        return Matchup?.IsPlayed != true;
    }

    private bool RefreshWinningView()
    {
        // Update the game play progress
        OnPropertyChanged(nameof(TournamentImageSource));
        OnPropertyChanged(nameof(IsPlayer1Winner));
        OnPropertyChanged(nameof(IsPlayer2Winner));
        OnPropertyChanged(nameof(IsComplete));

        OnPropertyChanged(nameof(NextMatch));
        OnPropertyChanged(nameof(WinningEntry));
        OnPropertyChanged(nameof(Player1Entry));
        OnPropertyChanged(nameof(Player2Entry));

        // Return true to continue the timer, false to stop it
        return Matchup?.IsPlayed != true;
    }

    private async Task StartNextGame()
    {
        IsBusy = true;
        Matchup = null;

        _player1CurtainCounter = 0;
        _player2CurtainCounter = 0;
        OnPropertyChanged(nameof(Player1CurtainTranslationX));
        OnPropertyChanged(nameof(Player2CurtainTranslationX));
        await Task.Delay(1000);

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
        // Start the game, Do not wait for it to finish
        _ = _communicator.Start(Matchup);
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
            await Shell.Current.GoToAsync($"//{nameof(LandingPage)}");
        }
    }

    TaskCompletionSource<Matchup> _tcs;
    public async Task<Matchup> StartMatchup(Matchup matchup, int countDown = 3)
    {
        IsBusy = true;
        await Task.Delay(1000);

        Tournament ??= (await _repository.Find(t => t.Rounds.Any(r => r.Any(m => m.Id == matchup.Id)))).FirstOrDefault();
        Matchup = Tournament?.FixParentMatchupRef()?.Matchups.FirstOrDefault(m => m?.Id == matchup?.Id) ?? matchup;

        _tcs = new TaskCompletionSource<Matchup>();

        if (await _communicator.Initialize() != true)
        {
            IsBusy = false;
            _tcs.SetException(new InvalidOperationException("Failed to initialize the game play! Check configs"));
            return await _tcs.Task;
        }

        IsBusy = false;

        // Refresh the bottle progress view when the matchup progress changes
        _communicator.OnMatchupProgressChanged += (sender, matchup) => RefreshProgressView();

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
                entry.StartTime ??= DateTime.UtcNow;

                // Hide count down text after a shot delay
                App.StartTimer(TimeSpan.FromSeconds(3), () => ShowCountDown = false);
            }
        };

        _communicator.OnPlayerWon += async (sender, player) =>
        {
            var entry = player.GetEntry(Matchup);

            _logger.LogInformation("{Player1} wins! {Distance:0.00}km in {Time}", player.Nickname, entry?.Distance, entry?.Time);

            // Update the game play progress
            RefreshWinningView();

            // Broudcast the game play progress
            WeakReferenceMessenger.Default.Send(new TournamentProgress(Tournament, Matchup, 100));
        };

        _communicator.OnMatchupFinished += async (sender, matchup) =>
        {
            if (Tournament == null) return;

            // Update the game play progress on match finish
            _logger.LogInformation("Matchup finished {Matchup}, Winner: {Winner}", matchup.ToString(), matchup.Winner?.Nickname);

            // Save the game play
            await Tournament?.Save();

            // Close the game play page
            await StopGame();

            // Calculate the game play progress based on match position
            var progress = 100.0 * Tournament.Matchups.IndexOf(Matchup) / Tournament.Matchups.Count();

            // Broudcast the game play progress
            WeakReferenceMessenger.Default.Send(new TournamentProgress(Tournament, null, progress));

            // Give the winner some time to celebrate
            await Task.Delay(5000);

            // Show match results
            ShowResults = true;
        };

        await StartCountDown(countDown);

        return await _tcs.Task;
    }
}
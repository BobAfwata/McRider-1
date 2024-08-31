
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
    [NotifyPropertyChangedFor(nameof(PlayCountDown))]
    [NotifyPropertyChangedFor(nameof(Player1Progress))]
    [NotifyPropertyChangedFor(nameof(Player2Progress))]
    [NotifyPropertyChangedFor(nameof(Player1ProgressF))]
    [NotifyPropertyChangedFor(nameof(Player2ProgressF))]
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
    //[NotifyCanExecuteChangedFor(nameof(ShowHorizontalProgress))]
    Tournament _tournament;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PlayCountDown))]
    int _countDown = 3;

    [ObservableProperty]
    bool _showCountDown = true;

    [ObservableProperty]
    bool _showResults;

    [ObservableProperty]
    double _curtainWidth = 500;

    [ObservableProperty]
    double _progressFillHeight = 580;

    [ObservableProperty]
    bool _isRunning = false;

    [ObservableProperty]
    DateTime? _startTime;

    double _player1CurtainCounter = 0;
    double _player2CurtainCounter = 0;

    public MatchupPageViewModel(ArdrinoCommunicator communicator, RepositoryService<Tournament> repository)
    {
        Title = "Game Play";

        _tournament = null;
        _communicator = communicator;
        _repository = repository;
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
                return PromoImage;

            var _tournamentImage = Tournament.CreateTournamentImage();
            if (_tournamentImage is not null)
                return ImageSource.FromStream(() => _tournamentImage.ToStream());

            return null;
        }
    }

    public ImageSource Player1ProgressImage => Matchup?.Player1?.Gender == "F"
        ? Theme.GetImage("Themes/progress_back_female.png", "./progress_back_female.png")
        : Theme.GetImage("Themes/progress_back_male.png", "./progress_back_male.png");

    public ImageSource Player2ProgressImage => Matchup?.Player2?.Gender == "F"
        ? Theme.GetImage("Themes/progress_back_female.png", "./progress_back_female.png")
        : Theme.GetImage("Themes/progress_back_male.png", "./progress_back_male.png");

    public bool IsComplete => Matchup?.IsPlayed == true;
    public bool IsSinglePlayer => IsMultiplePlayers != true;
    public bool IsMultiplePlayers => Matchup?.Players.DistinctBy(p => p?.Nickname).Count() > 1;

    public bool IsPlayer1Winner
    {
        get
        {
            if (Matchup?.IsPlayed != true)
                return false;

            if (Matchup?.Player1?.Id == null)
                return false;

            if (Matchup?.Player1?.GetEntry(Matchup)?.Distance < Tournament?.Game?.TargetDistance)
                return false;

            if (Matchup?.Winner?.Id == Matchup?.Player1?.Id)
                return true;

            return false;
        }
    }

    public bool IsPlayer2Winner
    {
        get
        {
            if (Matchup?.IsPlayed != true)
                return false;

            if (Matchup?.Player2?.Id == null)
                return false;

            if (Matchup?.Player2?.GetEntry(Matchup)?.Distance < Tournament?.Game?.TargetDistance)
                return false;

            if (Matchup?.Winner?.Id == Matchup?.Player2?.Id)
                return true;

            return false;
        }
    }

    public double PercentageTimeProgress => Matchup?.GetPercentageTimeProgress() ?? 0;
    public double Player1Progress => Matchup?.GetPlayersProgress(false).ElementAtOrDefault(0) ?? 0;
    public double Player2Progress => Matchup?.GetPlayersProgress(false).ElementAtOrDefault(1) ?? 0;

    public string PlayCountDown
    {
        get
        {
            if (CountDown > 0)
                return CountDown.ToString();

            var targetTime = Tournament?.Game?.TargetTime;
            var targetEndTime = StartTime?.Add(targetTime ?? TimeSpan.Zero);

            if (targetEndTime == null) return "0";

            var remainingTime = targetEndTime.Value - DateTime.UtcNow;
            if (IsComplete)
                remainingTime = Matchup?.Entries.Select(e => e.Time).Max() ?? TimeSpan.Zero;

            if (remainingTime.TotalMinutes > 60)
                return remainingTime.ToString(@"hh\:mm\:ss");

            if (remainingTime.TotalSeconds > 60)
                return remainingTime.ToString(@"mm\:ss");

            if (remainingTime.TotalSeconds < 1)
                return "Time up!";

            return remainingTime.ToString(@"ss").Replace("00", "0");
        }
    }

    public double Player1ProgressF => Player1Progress / 100.0;
    public double Player2ProgressF => Player2Progress / 100.0;

    public bool ShowHorizontalProgress => Tournament?.Game?.HorizontalProgress == true || Tournament?.Game?.GameType == GameType.DistanceChallenge;

    public double Player1ProgressFillHeight
    {
        get
        {
            if (Matchup == null)
                return 0;

            if (ShowHorizontalProgress)
                return ProgressFillHeight;

            var fillProgress = (App.Configs?.ReverseAnimation == true ? 100 - Player1Progress : Player1Progress);

            return fillProgress / 100.0 * ProgressFillHeight;
        }
    }

    public double Player2ProgressFillHeight
    {
        get
        {
            if (Matchup == null)
                return 0;

            if (ShowHorizontalProgress)
                return ProgressFillHeight;

            var fillProgress = (App.Configs?.ReverseAnimation == true ? 100 - Player2Progress : Player2Progress);

            return fillProgress / 100.0 * ProgressFillHeight;
        }
    }

    public double Player1CurtainTranslationX
    {
        get
        {
            if (Matchup == null || Tournament?.Game?.GameType != GameType.RevealChallenge)
                return 0;

            return -(Player1Progress / 100.0 * CurtainWidth / 2 - _player1CurtainCounter);
        }
    }

    public double Player2CurtainTranslationX
    {
        get
        {
            if (Matchup == null || Tournament?.Game?.GameType != GameType.RevealChallenge)
                return 0;

            if (IsSinglePlayer)
                return -Player1CurtainTranslationX;

            return (Player2Progress / 100.0 * CurtainWidth / 2 - _player2CurtainCounter);
        }
    }

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

        Matchup.Reset();
        RefreshProgressView();

        for (var i = countDown; i >= 0; i--)
        {
            CountDown = i;
            if (i >= 0) await Task.Delay(1200);
        }

        await StartGame();
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

        OnPropertyChanged(nameof(PlayCountDown));
        OnPropertyChanged(nameof(Player1Progress));
        OnPropertyChanged(nameof(Player2Progress));
        OnPropertyChanged(nameof(Player1ProgressF));
        OnPropertyChanged(nameof(Player2ProgressF));

        OnPropertyChanged(nameof(Player1Entry));
        OnPropertyChanged(nameof(Player2Entry));

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
        ShowResults = false;

        if (Tournament?.Game?.GameType == GameType.RevealChallenge)
        {
            // Close Curtains
            while (Player1CurtainTranslationX < 0 || Player2CurtainTranslationX > 0)
            {
                if (Player1CurtainTranslationX < 0)
                    _player1CurtainCounter += 5;
                if (Player2CurtainTranslationX > 0)
                    _player2CurtainCounter += 5;

                OnPropertyChanged(nameof(Player1CurtainTranslationX));
                OnPropertyChanged(nameof(Player2CurtainTranslationX));

                await Task.Delay(2);
            }
        }

        await Task.Delay(1000);
        await Shell.Current.GoToAsync($"///{nameof(StartGamePage)}");
        var vm = App.ServiceProvider.GetService<StartGamePageViewModel>();

        CountDown = Tournament?.Game?.CountDown ?? 3;
        ShowCountDown = true;
        Matchup = null;

        _player1CurtainCounter = 0;
        _player2CurtainCounter = 0;
        _communicator?.Stop();

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
        _ = _communicator.Start(Matchup).ContinueWith(async task =>
        {
            StartTime = DateTime.UtcNow;

            while (IsComplete == false && ShowResults == false)
            {
                await Task.Delay(1000);
                RefreshProgressView();
            }

            await Task.Delay(3000);
            StartTime = null;
        });
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

        _communicator.OnMatchupProgressChanged += (sender, matchup) => {
            if (IsRunning == false)
            {
                IsRunning = true;
                Task.Delay(3000).ContinueWith(t => ShowCountDown = false);
            }

            RefreshProgressView();

            // Broudcast the game play progress
            WeakReferenceMessenger.Default.Send(new TournamentProgress(Tournament, Matchup, Matchup?.GetPercentageProgress() ?? 0));
        };

        _communicator.OnPlayerDisconnected += async (sender, player) =>
        {
            // IsRunning = false;
            // TODO: Player disconnect notification
        };

        _communicator.OnPlayerStopped += async (sender, player) =>
        {
            // IsRunning = false;
            // TODO: Player stopped notification
        };

        _communicator.OnPlayerStart += async (sender, player) =>
        {
            var entry = player.GetEntry(Matchup);

            if (entry is not null)
                entry.StartTime ??= DateTime.UtcNow;

            if (IsRunning == false)
            {
                IsRunning = true;
                _ = Task.Delay(3000).ContinueWith(t => ShowCountDown = false);
            }
        };

        _communicator.OnPlayerWon += async (sender, player) =>
        {
            IsRunning = false;

            var entry = player.GetEntry(Matchup);

            _logger.LogInformation("{Player1} wins! {Distance:0.00}km in {Time}", player.Nickname, entry?.Distance, entry?.Time);

            // Update the game play progress
            RefreshWinningView();

            // Broudcast the game play progress
            WeakReferenceMessenger.Default.Send(new TournamentProgress(Tournament, Matchup, 100));
        };

        _communicator.OnMatchupFinished += async (sender, matchup) =>
        {
            IsRunning = false;

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
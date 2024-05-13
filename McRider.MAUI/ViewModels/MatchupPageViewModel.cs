
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
    [NotifyPropertyChangedFor(nameof(IsPlayer1Winner))]
    [NotifyPropertyChangedFor(nameof(IsPlayer2Winner))]
    [NotifyPropertyChangedFor(nameof(PercentageTimeProgress))]
    [NotifyPropertyChangedFor(nameof(Player1Progress))]
    [NotifyPropertyChangedFor(nameof(Player2Progress))]
    [NotifyPropertyChangedFor(nameof(Player1BottleProgress))]
    [NotifyPropertyChangedFor(nameof(Player2BottleProgress))]
    [NotifyPropertyChangedFor(nameof(NextMatch))]
    [NotifyPropertyChangedFor(nameof(WinningEntry))]
    [NotifyPropertyChangedFor(nameof(Player1Entry))]
    [NotifyPropertyChangedFor(nameof(Player2Entry))]

    Matchup _matchup;

    [ObservableProperty]
    Tournament _tournament;

    [ObservableProperty]
    int _countDown;

    [ObservableProperty]
    bool _showCountDown;


    [ObservableProperty]
    bool _showResults;

    public MatchupPageViewModel(ArdrinoCommunicator communicator, RepositoryService<Tournament> repository)
    {
        Title = "Game Play";
        _tournament = null;
        _communicator = communicator;
        _repository = repository;
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

    public bool IsComplete => IsPlayer1Winner || IsPlayer2Winner;
    public bool IsPlayer1Winner => Matchup?.Winner?.Id == Matchup?.Player1?.Id && Matchup?.Player1?.Id != null;
    public bool IsPlayer2Winner => Matchup?.Winner?.Id == Matchup?.Player2?.Id && Matchup?.Player2?.Id != null;
    public double PercentageTimeProgress => Matchup?.GetPercentageTimeProgress() ?? 0;
    public double Player1Progress => Matchup?.GetPlayersProgress().ElementAtOrDefault(0) ?? 0;
    public double Player2Progress => Matchup?.GetPlayersProgress().ElementAtOrDefault(1) ?? 0;
    public double Player1BottleProgress => Player1Progress / 100.0 * 580.0;
    public double Player2BottleProgress => Player2Progress / 100.0 * 580.0;

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
            Device.StartTimer(TimeSpan.FromSeconds(1), () => DoCountDown().Result);
    }

    private async Task<bool> DoCountDown()
    {
        var countDown = CountDown - 1;

        if (countDown <= 0)
            await StartGame();

        if (countDown < 0)
            ShowCountDown = false;

        if (countDown >= 0)
            CountDown = countDown;

        return countDown > 0;
    }

    private bool RefreshBottleProgressView()
    {
        _logger.LogInformation("Matchup progress changed. Progress {Progress:0.00}%", Matchup?.GetPercentageProgress());

        //Update the game play progress
        OnPropertyChanged(nameof(PercentageTimeProgress));
        OnPropertyChanged(nameof(Player1Progress));
        OnPropertyChanged(nameof(Player2Progress));
        OnPropertyChanged(nameof(Player1BottleProgress));
        OnPropertyChanged(nameof(Player2BottleProgress));

        // Broudcast the game play progress
        WeakReferenceMessenger.Default.Send(new TournamentProgress(Tournament, Matchup, Matchup?.GetPercentageProgress() ?? 0));

        // Return true to continue the timer, false to stop it
        return Matchup?.IsPlayed != true;
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

            var page = await Shell.Current.Navigation.PopAsync();

            while (page != null)
                page = await Shell.Current.Navigation.PopAsync();

            await Shell.Current.GoToAsync($"///{nameof(LandingPage)}");
        }
    }

    TaskCompletionSource<Matchup> _tcs;
    public async Task<Matchup> StartMatchup(Matchup matchup, int countDown = 3)
    {
        Tournament ??= (await _repository.Find(t => t.Rounds.Any(r => r.Any(m => m.Id == matchup.Id)))).FirstOrDefault();
        Matchup = Tournament?.FixParentMatchupRef()?.Matchups.FirstOrDefault(m => m?.Id == matchup?.Id) ?? matchup;

        _tcs = new TaskCompletionSource<Matchup>();
        IsBusy = true;

        if (await _communicator.Initialize() != true)
        {
            IsBusy = false;
            _tcs.SetException(new InvalidOperationException("Failed to initialize the game play! Check configs"));
            return await _tcs.Task;
        }

        IsBusy = false;

        // Check every second for game updates untill we have a winner
        //Device.StartTimer(TimeSpan.FromMilliseconds(100), RefreshBottleProgressView);

        // Refresh the bottle progress view when the matchup progress changes
        _communicator.OnMatchupProgressChanged += (sender, matchup) => RefreshBottleProgressView();

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

            _logger.LogInformation("{Player1} wins! {Distance:0.00} {Time:hh:mm:ss}", player.Name, entry?.Time, entry?.Distance);

            // Update the game play progress
            OnPropertyChanged(nameof(TournamentImageSource));
            OnPropertyChanged(nameof(IsPlayer1Winner));
            OnPropertyChanged(nameof(IsPlayer2Winner));
            OnPropertyChanged(nameof(IsComplete));

            // Broudcast the game play progress
            WeakReferenceMessenger.Default.Send(new TournamentProgress(Tournament, Matchup, 100));
        };

        _communicator.OnMatchupFinished += async (sender, matchup) =>
        {
            // Update the game play progress
            OnPropertyChanged(nameof(TournamentImageSource));

            // Save the game play
            await Tournament?.Save();

            // Close the game play page
            await StopGame();

            // Calculate the game play progress based on match position
            var progress = 100.0 * Tournament.Matchups.IndexOf(Matchup) / (double)Tournament.Matchups.Count();

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
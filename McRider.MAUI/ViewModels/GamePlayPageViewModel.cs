using McRider.MAUI.Services;

namespace McRider.MAUI.ViewModels;

public partial class GamePlayPageViewModel : BaseViewModel
{
    private ArdrinoCommunicator _communicator;

    [ObservableProperty]
    GamePlay _gamePlay;

    public GamePlayPageViewModel(ArdrinoCommunicator communicator)
    {
        Title = "Game Play";
        _communicator = communicator;
    }

    public double Player1BottleProgress => (GamePlay?.Player1Progress ?? 0) * 580.0 / 100.0;
    public double Player2BottleProgress => (GamePlay?.Player2Progress ?? 0) * 580.0 / 100.0;

    [RelayCommand]
    private async Task StopGame()
    {
        // Stop the game
        await _communicator.Stop();
    }

    TaskCompletionSource<GamePlay> _tcs;

    public async Task<GamePlay> StartGamePlay(GamePlay gamePlay)
    {
        GamePlay = gamePlay;
        _tcs = new TaskCompletionSource<GamePlay>();

        // Check every second for game updates untill we have a winner
        App.StartTimer(TimeSpan.FromSeconds(1), () =>
        {
            OnPropertyChanged(nameof(GamePlay));
            OnPropertyChanged(nameof(Player1BottleProgress));
            OnPropertyChanged(nameof(Player2BottleProgress));

            // Return true to continue the timer, false to stop it
            return GamePlay.Winner == null;
        });

        _communicator.OnPlayerDisconnected += async (sender, player) =>
        {
            
        };

        _communicator.OnPlayerStopped += async (sender, player) =>
        {
            
        }; 
        
        _communicator.OnPlayerStart += async (sender, player) =>
        {
            player.StartTime ??= DateTime.UtcNow;
        };

        _communicator.OnPlayerWon += async (sender, player) =>
        {
            player.IsWinner = true;
        };

        _communicator.OnGamePlayFinished += async (sender, e) =>
        {
            // Close the game play page
            await StopGame(); 
            
            // Give the winner some time to celebrate
            await Task.Delay(TimeSpan.FromSeconds(20));

            // Return the game play
            _tcs.SetResult(gamePlay);
        };
        
        await _communicator.Start(gamePlay);

        return await _tcs.Task;
    }
}
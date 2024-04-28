namespace McRider.MAUI.ViewModels;

public partial class StartGamePageViewModel : BaseViewModel
{
    [ObservableProperty]
    GamePlay _gamePlay;

    [ObservableProperty]
    bool _twoPlayerMode;

    partial void OnGamePlayChanged(GamePlay? oldValue, GamePlay newValue)
    {
        TwoPlayerMode = newValue?.Player2?.IsActive == true;
    }

    [RelayCommand]
    private async Task StartGame()
    {
        if (IsValid(GamePlay) != true)
            return;

        _tcs.SetResult();
    }

    private bool IsValid(GamePlay gamePlay)
    {
        return gamePlay != null && gamePlay.Player1 != null && gamePlay.Player2 != null;
    }

    TaskCompletionSource _tcs;
    public async Task<List<GamePlay>> AwaitGamePlaysFor(Player[] players, GameItem game)
    {
        var teamsArray = players.GroupBy(p => p.Team).ToArray();
        var gamePlays = new List<GamePlay>();

        if (players.Length <= 0)
            throw new InvalidOperationException("At least one player is required to start a game.");
        else if (players.Length == 1)
            gamePlays.Add(new GamePlay { Game = game, Player1 = players.FirstOrDefault(), Player2 = new Player() { IsActive = false } });
        else
            // Schedule game plays so that each player plays once with a player of another team
            for (int i = 0; i < teamsArray.Length; i++)
            {
                for (int j = i + 1; j < teamsArray.Length; j++)  // Ensure pairing between different teams
                {
                    var team1Players = teamsArray[i].ToArray();
                    var team2Players = teamsArray[j].ToArray();

                    foreach (var player1 in team1Players)
                    {
                        foreach (var player2 in team2Players)
                        {
                            gamePlays.Add(new GamePlay
                            {
                                Game = game,
                                Player1 = player1,
                                Player2 = player2
                            });
                        }
                    }
                }
            }

        // Play each game
        foreach (var play in gamePlays)
        {
            GamePlay = play;
            _tcs = new TaskCompletionSource();
            await _tcs.Task;
            await GamePlayPage.StartGamePlay(GamePlay);
        }

        return gamePlays;
    }
}

public class GamePlay
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public GameItem Game { get; set; }

    public Player Player1 { get; set; }
    public Player Player2 { get; set; }

    public Player? Winner
    {
        get
        {
            if (PercentageProgress < 100) 
                return null;

            if (Player1.Distance > Player2.Distance)
                return Player1;
            else if (Player1.Distance < Player2.Distance)
                return Player2;
            else if (Player1.Time < Player2.Time)
                return Player1;
            else if (Player1.Time > Player2.Time)
                return Player2;

            return null;
        }
    }

    public double Player1Progress => GetPercentageProgress(Player1, Game, false);
    public double Player2Progress => GetPercentageProgress(Player2, Game, false);

    public double PercentageTimeProgress => Math.Max(
        GetPercentageProgress(Player1, Game, null),
        GetPercentageProgress(Player2, Game, null)
    );

    public double PercentageProgress => Math.Max(
        GetPercentageProgress(Player1, Game),
        GetPercentageProgress(Player2, Game)
    );

    public static double GetPercentageProgress(Player player, GameItem game, bool? bestOfDistanceVsTime = true)
    {
        if (player == null || player.Distance <= 0) return 0;
        if (game == null) return 0;

        var distanceProgress = game?.TargetDistance <= 0 ? 0 : player.Distance / game?.TargetDistance;
        var timeProgress = game?.TargetTime?.TotalMicroseconds <= 0 ? 0 : player.Time?.TotalMicroseconds / game?.TargetTime?.TotalMicroseconds;

        App.Logger?.LogInformation($"{player.Nickname} Distance progress: {distanceProgress:0.00}, Time progress: {timeProgress:0.00}");

        if (bestOfDistanceVsTime == true)
            return Math.Round(Math.Min(100.0, Math.Max(distanceProgress ?? 0, timeProgress ?? 0) * 100), 2);
        
        if (bestOfDistanceVsTime == false)
            return Math.Round(Math.Min(100.0, (distanceProgress ?? 0) * 100), 2);

        return Math.Round(Math.Min(100.0, (timeProgress ?? 0) * 100), 2);
    }
}

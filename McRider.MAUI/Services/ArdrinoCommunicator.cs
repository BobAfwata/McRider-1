using McRider.Common.Services;

namespace McRider.MAUI.Services;

public abstract class ArdrinoCommunicator
{
    const int ACTIVITY_DISCONECT_TIMEOUT = 3000;
    const int ACTIVITY_ENDED_TIMEOUT = 15000;

    protected ILogger _logger;

    protected FileCacheService _cacheService;
    protected GamePlay _gamePlay;
    protected Configs _configs;
    protected bool _isRunning = true;

    public ArdrinoCommunicator(FileCacheService cacheService, ILogger<ArdrinoCommunicator> logger = null)
    {
        _cacheService = cacheService;
        _logger = logger;

        _ = Initialize();
    }

    public virtual async Task Initialize()
    {
        _configs = await _cacheService.GetAsync<Configs>("configs.json", async () => new Configs());
    }

    public abstract string ReadData();
    public abstract void SendData(string data);

    public bool IsRunning
    {
        get
        {
            if (_isRunning != true)
                return false;

            if (!IsActive(_gamePlay.Player1) && !IsActive(_gamePlay.Player2))
                return _isRunning = false;

            return true;
        }
    }

    public Action<object, GamePlay> OnGamePlayFinished { get; set; }
    public Action<object, Player> OnPlayerWon { get; set; }
    public Action<object, Player> OnPlayerDisconnected { get; set; }
    public Action<object, Player> OnPlayerStopped { get; set; }
    public Action<object, Player> OnPlayerStart { get; set; }

    public virtual Task Stop() => Task.FromResult(_isRunning = false);

    public async Task Start(GamePlay gamePlay)
    {
        _gamePlay = gamePlay ?? throw new ArgumentNullException(nameof(gamePlay));
        _gamePlay.Player1.Reset();
        _gamePlay.Player2.Reset();

        _isRunning = true;
#if DEBUG
        await Task.Run(() => DoFakeReadData());
#else
        await Task.Run(() => DoReadData());
#endif

    }

    private bool IsActive(Player player)
    {
        var lastActivity = player.LastActivity;
        if (lastActivity.HasValue)
        {
            var delay = (DateTime.UtcNow - lastActivity.Value).Milliseconds;

            if (delay > ACTIVITY_ENDED_TIMEOUT)
            {
                OnPlayerStopped?.Invoke(this, player);
                return false;
            }

            if (delay > ACTIVITY_DISCONECT_TIMEOUT)
                OnPlayerDisconnected?.Invoke(this, player);
        }

        // End the game when target Distance/Time reached by one of the players
        var progress = _gamePlay.PercentageProgress;
        if (progress >= 100)
        {
            var winner = _gamePlay.Winner;
            if (winner != null)
                OnPlayerWon?.Invoke(this, winner);

            if (_gamePlay.Game.AllowLosserToFinish == true)
                return false;
            else
                return _isRunning = false;
        }

        return true;
    }

    private void UpdatePlayerDistance(Player player, double distance)
    {
        if (player.IsActive != true)
            return;

        var playerDelta = Math.Abs(player.Distance - distance);

        if (player.Distance <= 0 && playerDelta > 0)
            OnPlayerStart?.Invoke(this, _gamePlay.Player1);

        player.Distance = distance;
    }

    private void DoFakeReadData()
    {
        double minValue = 5, maxValue = 20;

        while (IsRunning)
        {
            Thread.Sleep(200);
            var player1Delta = Random.Shared.NextDouble() * (maxValue - minValue) + minValue;
            var player2Delta = Random.Shared.NextDouble() * (maxValue - minValue) + minValue;

            UpdatePlayerDistance(_gamePlay.Player1, _gamePlay.Player1.Distance + player1Delta);
            UpdatePlayerDistance(_gamePlay.Player2, _gamePlay.Player2.Distance + player2Delta);
        }

        OnGamePlayFinished?.Invoke(this, _gamePlay);
    }

    public void DoReadData()
    {
        double start_counter_a = 0, start_counter_b = 0;

        while (IsRunning)
        {
            try
            {
                var message = ReadData();
                var json_object = JObject.Parse(message.ToString());
                var strDistance1 = (string)json_object["distance1"];
                var strDistance2 = (string)json_object["distance2"];

                double distance1 = 0, distance2 = 0;

                if (strDistance1 != null)
                {
                    var bike_a = Convert.ToDouble(strDistance1);
                    var bike_b = Convert.ToDouble(strDistance2);
                    if (start_counter_a == 0)
                    {
                        start_counter_a = bike_a;
                        start_counter_b = bike_b;
                    }
                    else
                    {
                        distance1 = double.Parse(strDistance1) - start_counter_a / 1000.0;
                        distance2 = double.Parse(strDistance2) - start_counter_b / 1000.0;
                    }
                }
                else
                {
                    int bike_a = Convert.ToInt32(json_object["bikeA"]);
                    int bike_b = Convert.ToInt32(json_object["bikeB"]);


                    if (start_counter_a == 0)
                    {
                        start_counter_a = bike_a;
                        start_counter_b = bike_b;
                    }
                    else
                    {
                        distance1 = 0.622 * (bike_a - start_counter_a) / 1000.0;
                        distance2 = 0.622 * (bike_b - start_counter_b) / 1000.0;
                    }
                }

                UpdatePlayerDistance(_gamePlay.Player1, distance1);
                UpdatePlayerDistance(_gamePlay.Player2, distance2);
            }
            catch
            {
                //  MessageBox.Show(ex.Message);
            }
        }
    }
}

public class Configs
{
    public string PortName { get; set; } = "COM7";
    public int BaudRate { get; set; } = 9600;
    public int ReadTimeout { get; set; } = 5000;
}

namespace McRider.MAUI.Services;

public abstract class ArdrinoCommunicator
{
    const int ACTIVITY_DISCONECT_TIMEOUT = 3000;
    const int ACTIVITY_ENDED_TIMEOUT = 15000;

    protected ILogger _logger;

    protected FileCacheService _cacheService;
    protected Matchup _matchup;
    protected Configs _configs;
    protected bool _isRunning = true;

    public ArdrinoCommunicator(FileCacheService cacheService, ILogger<ArdrinoCommunicator> logger = null)
    {
        _cacheService = cacheService;
        _logger = logger;

        _ = Initialize();
    }

    public virtual async Task<bool> Initialize()
    {
        _configs = await _cacheService.GetAsync<Configs>("configs.json", async () => new Configs());
        return true;
    }

    public abstract string ReadData();
    public abstract void SendData(string data);

    public bool IsRunning
    {
        get
        {
            if (_isRunning != true)
                return false;

            if (!IsActive(_matchup.Player1) && !IsActive(_matchup.Player2))
                return _isRunning = false;

            return true;
        }
    }

    public Action<object, Matchup> OnMatchupFinished { get; set; }
    public Action<object, Player> OnPlayerWon { get; set; }
    public Action<object, Player> OnPlayerDisconnected { get; set; }
    public Action<object, Player> OnPlayerStopped { get; set; }
    public Action<object, Player> OnPlayerStart { get; set; }

    public virtual Task Stop() => Task.FromResult(_isRunning = false);

    public virtual async Task Start(Matchup matchup)
    {
        _matchup = matchup ?? throw new ArgumentNullException(nameof(matchup));
        _matchup.Reset();

        _isRunning = true;
#if DEBUG
        await Task.Run(() => DoFakeReadData());
#else
        await Task.Run(() => DoReadData());
#endif

    }

    private bool IsActive(Player player)
    {
        var lastActivity = player?.GetEntry(_matchup)?.LastActivity;
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
        var progress = _matchup.GetPercentageProgress();
        if (progress >= 100)
        {
            var winner = _matchup.GetWinner();
            if (winner != null)
                OnPlayerWon?.Invoke(this, winner); // Notify the winner

            if (_matchup.Game.AllowLosserToFinish == true)
                return false; // Allow the loser to finish
            else
                return _isRunning = false; // End the game
        }

        return true;
    }

    private void UpdatePlayerDistance(MatchupEntry entry, double distance)
    {
        var playerDelta = Math.Abs((entry?.Distance ?? 0) - distance);

        if (entry?.Player == null)
            return;

        if (entry?.Player != null && entry.Distance <= 0 && playerDelta > 0)
            OnPlayerStart?.Invoke(this, entry?.Player);

        if (entry != null)
            entry.Distance = distance;
    }

    private void DoFakeReadData()
    {
        double minValue = 5, maxValue = 20;

        while (IsRunning)
        {
            Thread.Sleep(200);

            foreach (var entry in _matchup.Entries)
            {
                var delta = Random.Shared.NextDouble() * (maxValue - minValue) + minValue;
                UpdatePlayerDistance(entry, entry.Distance + delta);
            }
        }

        OnMatchupFinished?.Invoke(this, _matchup);
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


                if (_matchup.Entries.Count > 0)
                    UpdatePlayerDistance(_matchup.Entries[0], distance1);
                if (_matchup.Entries.Count > 1)
                    UpdatePlayerDistance(_matchup.Entries[1], distance2);
            }
            catch
            {
                //  MessageBox.Show(ex.Message);
            }
        }
    }
}
using CommunityToolkit.Mvvm.Messaging;
using McRider.Domain.Models;

namespace McRider.MAUI.Services;

public abstract class ArdrinoCommunicator
{
    const int ACTIVITY_DISCONECT_TIMEOUT = 3000;
    const int ACTIVITY_ENDED_TIMEOUT = 15000;

    protected ILogger _logger;

    protected FileCacheService _cacheService;
    protected Matchup _matchup;
    protected Configs _configs;
    protected bool _isRunning = false;

    public ArdrinoCommunicator(FileCacheService cacheService, ILogger<ArdrinoCommunicator>? logger = null)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public virtual async Task<bool> Initialize()
    {
        _configs = (await _cacheService.GetAsync("configs.json", () => Task.FromResult(new[] { new Configs() }))).FirstOrDefault();
        return true;
    }

    public abstract Task<string?> ReadDataAsync(TimeSpan? timeout = null, int retryCount = 0);

    public virtual Task ClearBuffer(TimeSpan? timeout = null)
    {
        // Nothing to do here. Implement in derived class
        return Task.CompletedTask;
    }

    public abstract void SendData(string data);

    public bool IsRunning
    {
        get
        {
            if (_isRunning != true)
                return false;

            if (IsActive(_matchup.Player1) == false && IsActive(_matchup.Player2) == false && _matchup.IsPlayed == true)
                return _isRunning = false;

            return true;
        }
    }

    public Action<object, Matchup> OnMatchupProgressChanged { get; set; }
    public Action<object, Matchup> OnMatchupFinished { get; set; }
    public Action<object, Player> OnPlayerWon { get; set; }
    public Action<object, Player[]> OnMatchupTired { get; set; }
    public Action<object, Player> OnPlayerDisconnected { get; set; }
    public Action<object, Player> OnPlayerStopped { get; set; }
    public Action<object, Player> OnPlayerStart { get; set; }

    public virtual Task Stop() => Task.FromResult(_isRunning = false);

    public virtual async Task Start(Matchup matchup)
    {
        if (_isRunning)
            return;

        _isRunning = true;

        _matchup = matchup ?? throw new ArgumentNullException(nameof(matchup));
        _matchup.Reset();

        await DoReadDataAsync();
    }

    private bool CheckMatchupState(MatchupEntry entry)
    {
        if (entry == null) return false;

        var lastActivity = entry.LastActivity;
        if (lastActivity.HasValue)
        {
            var delay = (DateTime.UtcNow - lastActivity.Value).TotalMilliseconds;

            if (delay > ACTIVITY_ENDED_TIMEOUT)
            {
                entry.IsActive = false;
                OnPlayerStopped?.Invoke(this, entry.Player);
                return false;
            }
            else if (delay > ACTIVITY_DISCONECT_TIMEOUT)
            {
                OnPlayerDisconnected?.Invoke(this, entry.Player);
                return false;
            }
        }

        var timeMet = _matchup.TargetEndTime.HasValue && _matchup.TargetEndTime <= DateTime.UtcNow;
        var distanceMet = _matchup.Game.TargetDistance.HasValue && _matchup.Game.TargetDistance <= entry.Distance;

        // If the distance and time are not met, return true
        if (!distanceMet && !timeMet)
            return true;

        // Mark the matchup as played
        _matchup.IsPlayed = true;

        // Mark the entry as inactive
        entry.IsActive = false;

        // Notify the winner
        var winner = _matchup.Winner;
        if (_isRunning && winner != null)
            OnPlayerWon?.Invoke(this, winner); // Notify the winner
        else if (_matchup.Loser == null)
            OnMatchupTired?.Invoke(this, _matchup.Players.ToArray());

        if (_matchup.Game?.AllowLosserToFinish == true)
            return false; // Return false but allow the loser to finish
        else
            return _isRunning = false; // Return and End the game
    }

    private bool IsActive(Player player)
    {
        var entry = player?.GetEntry(_matchup);
        if (entry == null) return false;

        // Check for inactivity.
        CheckMatchupState(entry);

        return entry.IsActive;
    }

    private void UpdatePlayerDistance(MatchupEntry entry, double distance)
    {
        var delta = Math.Abs((entry?.Distance ?? 0) - distance);

        if (entry?.Player == null)
            return;

        entry.Distance = distance;

        if (delta > 0)
        {
            entry.IsActive = true;
            if (entry.Distance <= 0)
            {
                entry.StartTime ??= DateTime.UtcNow;
                if (entry?.Player != null)
                    OnPlayerStart?.Invoke(this, entry?.Player);
            }
        }

        CheckMatchupState(entry);
    }

    protected async Task DoFakeReadData()
    {
        double minValue = 1, maxValue = 5;
        var startTime = DateTime.UtcNow;
        var stopTimeout = 10;
        var pauseActivity = false;

        App.StartTimer(TimeSpan.FromMilliseconds(100), () =>
        {
            var progressChanged = false;

            try
            {
                var seconds = Math.Ceiling((DateTime.UtcNow - startTime).TotalSeconds);
                if (stopTimeout > 0 && seconds % stopTimeout == 0)
                    pauseActivity = true;
                else if (stopTimeout > 0 && seconds % Math.Ceiling(stopTimeout * 1.5) == 0)
                    pauseActivity = false;

                foreach (var entry in _matchup.Entries)
                {
                    // Set delta to 0 if the activity is paused
                    var delta = pauseActivity ? 0 : Random.Shared.NextDouble() * (maxValue - minValue) + minValue;

                    // Randomly slow down one player
                    if (Random.Shared.NextDouble() < 0.1)
                        delta = 0;

                    progressChanged = delta > 0;
                    UpdatePlayerDistance(entry, entry.Distance + delta);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error reading data from the ardrino");
            }
            finally
            {
                if (progressChanged)
                    OnMatchupProgressChanged?.Invoke(this, _matchup);

                if (!IsRunning)
                    OnMatchupFinished?.Invoke(this, _matchup);
            }

            return IsRunning;
        });
    }

    public virtual async Task DoReadDataAsync()
    {
        double start_counter_a = 0, start_counter_b = 0;
        long count = 0;

        App.StartTimer(TimeSpan.FromMilliseconds(100), () =>
        {
            var progressChanged = false;
            try
            {
                var message = ReadDataAsync().Result;
                if (string.IsNullOrEmpty(message))
                    return IsRunning == true;

                _logger.LogDebug(message);

                var json = JObject.Parse(message.ToString());

                var strDistance1 = (string)json["distance_1"];
                var strDistance2 = (string)json["distance_2"];

                double distance1 = 0, distance2 = 0;

                if (strDistance1 != null)
                {
                    var counter_a = Convert.ToDouble(strDistance1);
                    var counter_b = Convert.ToDouble(strDistance2);
                    if (start_counter_a == 0)
                    {
                        start_counter_a = counter_a;
                        start_counter_b = counter_b;
                    }
                    else
                    {
                        distance1 = (counter_a - start_counter_a);
                        distance2 = (counter_b - start_counter_b);
                    }
                }
                else if (json["bikeA"] != null)
                {
                    int bike_a = Convert.ToInt32(json["bikeA"]);
                    int bike_b = Convert.ToInt32(json["bikeB"]);

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
                {
                    progressChanged = progressChanged || _matchup.Entries[0].Distance < distance1;
                    UpdatePlayerDistance(_matchup.Entries[0], distance1);
                }

                if (_matchup.Entries.Count > 1)
                {
                    progressChanged = progressChanged || _matchup.Entries[1].Distance < distance2;
                    UpdatePlayerDistance(_matchup.Entries[1], distance2);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error reading data from the ardrino");
            }
            finally
            {
                if (progressChanged)
                    OnMatchupProgressChanged?.Invoke(this, _matchup);

                if (!IsRunning)
                    OnMatchupFinished?.Invoke(this, _matchup);
            }

            return IsRunning;
        });
    }
}
namespace McRider.Domain.Models;

public class MatchupEntry : IComparable<MatchupEntry>
{
    private double _distance;
    private Matchup? parentMatchup;

    public string? Id { get; set; } = "matchupEntry-" + Guid.NewGuid().ToString();
    public Matchup? ParentMatchup
    {
        get => parentMatchup;
        set
        {
            parentMatchup = value;
            Player ??= parentMatchup?.Winner;
        }
    }
    public Player? Player { get; set; }

    [JsonIgnore]

    public DateTime? StartTime { get; set; }
    public DateTime? LastActivity { get; set; }

    [JsonIgnore]
    public TimeSpan? Time
    {
        get
        {
            if (StartTime.HasValue == false) return null;
            return LastActivity - StartTime;
        }
    }

    [JsonIgnore]
    public double Distance
    {
        get => _distance;
        set
        {
            var delta = Math.Abs(_distance - value);
            if (delta <= 0.001) return;

            if (StartTime.HasValue == false)
                StartTime = DateTime.UtcNow;

            LastActivity = DateTime.UtcNow;
            _distance = value;
        }
    }

    public bool IsWinner { get; set; }

    public override string ToString() => $"{Player?.Nickname} ({Distance}km - {Time?.ToString("mm:ss")})";

    public void Reset()
    {
        _distance = 0;
        StartTime = null;
        LastActivity = null;
    }

    public int CompareTo(MatchupEntry? other)
    {
        if (other is null)
            return 1; // Player 1 wins by default
        else if (Distance > other?.Distance)
            return 1; // Player 1 wins by distance covered
        else if (Distance < other?.Distance)
            return -1; // Player 2 wins by distance covered
        else if (Time < other?.Time)
            return 1; // Player 1 wins by time taken
        else if (Time > other?.Time)
            return -1;

        return 0; // It's a tie
    }

    public static bool operator ==(MatchupEntry? left, MatchupEntry? right) => left?.CompareTo(right) == 0;
    public static bool operator !=(MatchupEntry? left, MatchupEntry? right) => left?.CompareTo(right) != 0;
    public static bool operator <(MatchupEntry? left, MatchupEntry? right) => left?.CompareTo(right) < 0;
    public static bool operator >(MatchupEntry? left, MatchupEntry? right) => left?.CompareTo(right) > 0;

}


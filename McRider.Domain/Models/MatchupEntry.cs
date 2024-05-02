namespace McRider.Domain.Models;

public class MatchupEntry : IComparable<MatchupEntry>
{
    private double _distance;
    private Player? _player;

    public MatchupEntry(Matchup parentMatchup, Matchup currentMatchup)
    {
        ParentMatchup = parentMatchup;
        CurrentMatchup = currentMatchup;
    }

    public string? Id { get; set; } = "matchupEntry-" + Guid.NewGuid().ToString();

    public Matchup? CurrentMatchup { get; set; } // The matchup that this entry came from
    public Matchup? ParentMatchup { get; set; } // The matchup that this entry came from 

    public Player? Player
    {
        get
        {
            if (_player is null && CurrentMatchup is not null)
            {
                if (CurrentMatchup.Bracket == Bracket.GrandFinals || ParentMatchup?.Bracket == CurrentMatchup.Bracket)
                    _player = ParentMatchup.Winner;
                else if (ParentMatchup?.Bracket == Bracket.Winners && CurrentMatchup.Bracket == Bracket.Losers)
                    _player = ParentMatchup.Loser;
                else
                    _player = null;
            }

            return _player;
        }
        set => _player = value;
    }

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

    public bool? IsWinner => CurrentMatchup?.IsComplete == true &&   CurrentMatchup?.Winner?.Id == Id;

    public override string ToString()
    {
        var output = $"{Player?.Nickname}";
        if (Distance > 0)
            output = $"({Distance}m - {Time?.ToString("mm:ss")})";

        return output;
    }

    public void Reset()
    {
        _distance = 0;
        StartTime = null;
        LastActivity = null;
    }

    public int CompareTo(MatchupEntry? other)
    {
        if (this is null && other is null)
            return 0; // It's a tie at null :)
        else if (other is null || Distance > other?.Distance)
            return -1; // Player 1 wins by distance covered
        else if (this is null || Distance < other?.Distance)
            return 1; // Player 2 wins by distance covered
        else if (Time < other?.Time)
            return -1; // Player 1 wins by time taken
        else if (Time > other?.Time)
            return 1;

        return 0; // It's a tie
    }
}


using System;

namespace McRider.Domain.Models;

public class MatchupEntry : IComparable<MatchupEntry>
{
    private double _distance;
    private Player? _player;

    public MatchupEntry(Matchup currentMatchup, Matchup parentMatchup = null)
    {
        CurrentMatchup = currentMatchup;
        ParentMatchup = parentMatchup;
    }

    public string? Id { get; set; } = "matchupEntry-" + Guid.NewGuid().ToString();
    public bool? IsWinner => CurrentMatchup?.IsComplete == true && CurrentMatchup?.Winner?.Id == Player?.Id;

    public Matchup? CurrentMatchup { get; set; } // The matchup that this entry came from
    public Matchup? ParentMatchup { get; set; } // The matchup that this entry came from 

    public Player? Player
    {
        get
        {
            if (_player == null && CurrentMatchup != null && ParentMatchup?.IsByeMatchup != true)
            {
                // Check if we're in the Grand Finals
                if (CurrentMatchup.Bracket == Bracket.GrandFinals)
                {
                    // Grand finals must have a ParentMatchup
                    if (ParentMatchup == null)
                        throw new InvalidOperationException("Grand finals must have a parent matchup");

                    // Check if ParentMatchup has no players
                    if (ParentMatchup.Players.Count() <= 0)
                    {
                        // If there are no players in ParentMatchup, there's no valid player to return
                        _player = null;
                    }
                    else if (ParentMatchup.Entries.FirstOrDefault(e => e.IsWinner == true)?.ParentMatchup?.Bracket == Bracket.Winners)
                    {
                        // Check if set1 finals winner was in the Winners bracket
                        _player = null;
                    }
                    else
                    {
                        // If the above conditions are not met, assign the player to the winner of ParentMatchup
                        _player = ParentMatchup.Winner;
                    }
                }
                else if (ParentMatchup?.Bracket == Bracket.Winners && CurrentMatchup.Bracket == Bracket.Losers)
                {
                    // Player dropped from winners to losers bracket
                    _player = ParentMatchup?.Loser;
                }
                else if (ParentMatchup?.Bracket == CurrentMatchup.Bracket)
                {
                    // In the same bracket
                    _player = ParentMatchup?.Winner;
                }
                else
                {
                    // Any other case
                    _player = null;
                }
            }

            return _player;
        }
        set => _player = value;
    }

    public DateTime? StartTime { get; set; }
    public DateTime? LastActivity { get; set; }

    public TimeSpan? Time
    {
        get
        {
            if (StartTime.HasValue == false) return null;
            return LastActivity - StartTime;
        }
    }

    public double Distance
    {
        get => _distance;
        set
        {
            var delta = Math.Abs(_distance - value);
            if (delta <= 0.001) return; // Ignore small changes

            // If the matchup is not complete, then we need to update the last activity time
            if (CurrentMatchup?.IsComplete != true)
            {
                LastActivity = DateTime.UtcNow;

                // If the start time is not set, then set it to the current time
                if (StartTime.HasValue == false)
                    StartTime = LastActivity;
            }

            _distance = value;
        }
    }


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


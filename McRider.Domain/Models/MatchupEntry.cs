using System;
using System.Reflection;

namespace McRider.Domain.Models;

public class MatchupEntry : IComparable<MatchupEntry>
{
    private double _distance;
    private Player? _player;

    public MatchupEntry(Matchup currentMatchup, Matchup? parentMatchup = null)
    {
        CurrentMatchup = currentMatchup;
        ParentMatchup = parentMatchup;
    }

    public string? Id { get; set; } = "matchupEntry-" + Guid.NewGuid().ToString();
    public bool? IsWinner => CurrentMatchup?.IsPlayed == true && CurrentMatchup?.Winner?.Id == Player?.Id;

    public Matchup? CurrentMatchup { get; set; } // The matchup that this entry came from
    public Matchup? ParentMatchup { get; set; } // The matchup that this entry came from 

    public Player? Player
    {
        get
        {

            if ((ParentMatchup?.IsPlayed == false && ParentMatchup?.IsByeMatchup == false) || _player is not null)
                return _player;

            if (CurrentMatchup?.Bracket == Bracket.GrandFinals)
            {
                if (ParentMatchup?.Bracket == Bracket.GrandFinals)
                {
                    // GrandFinals Set 2
                    var parentEntries = ParentMatchup.Entries;
                    var indexEntry = CurrentMatchup?.Entries.IndexOf(x => x.Id == Id) ?? -1;
                    var winEntry = parentEntries.FirstOrDefault(e => e.IsWinner == true);
                    if (winEntry == null || winEntry?.ParentMatchup?.Bracket == Bracket.Winners)
                        return null;

                    return _player = ParentMatchup.GetPlayerAt(indexEntry);
                }
            }

            if (CurrentMatchup?.Bracket == Bracket.Winners)
                _player = ParentMatchup?.Winner;
            else if (CurrentMatchup?.Bracket == Bracket.GrandFinals)
                _player = ParentMatchup?.Winner;
            else if (ParentMatchup?.Bracket == Bracket.Losers)
                _player = ParentMatchup?.Winner;
            else
                _player = ParentMatchup?.Loser;

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
            if (delta <= 0.009) return; // Ignore small changes

            // If the matchup is not complete, then we need to update the last activity time
            if (CurrentMatchup?.IsPlayed != true)
            {
                LastActivity = DateTime.UtcNow;

                // If the start time is not set, then set it to the current time
                if (StartTime.HasValue == false)
                    StartTime = LastActivity;
            }

            _distance = value;
        }
    }

    public bool ExpectsPlayerEntry
    {
        get
        {
            // If the player is already set, then we don't expect more players
            if (_player != null)
                return false;

            // If the matchup exists and is complete, then we don't expect more players
            if (CurrentMatchup?.IsPlayed == true)
                return false;

            // If the matchup is in Round 1 of the Winners bracket, then we don't expect more players
            if (CurrentMatchup?.Round == 1 && CurrentMatchup.Bracket == Bracket.Winners)
                return false;

            // Check recursively if any parent matchups expect players
            if (ParentMatchup?.ParentMatchups?.Any(p => p.Entries.Any(e => e.ExpectsPlayerEntry)) == true)
                return true;

            // Default to false if no conditions indicate that players are expected
            return false;
        }
    }

    public bool IsDroppedBrackets() => ParentMatchup != null && ParentMatchup?.Bracket != CurrentMatchup?.Bracket;

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
#if DEBUG
        else if (Time < other?.Time)
            return -1; // Player 1 wins by time taken
        else if (Time > other?.Time)
            return 1;
#else
        else if ((int)Time.Value.TotalSeconds < (int)other?.Time.Value.TotalSeconds)
            return -1; // Player 1 wins by time taken
        else if ((int)Time.Value.TotalSeconds > (int)other?.Time.Value.TotalSeconds)
            return 1;
#endif


        return 0; // It's a tie
    }
}


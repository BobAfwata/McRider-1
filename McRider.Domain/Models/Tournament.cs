using System.Collections.Concurrent;

namespace McRider.Domain.Models;

public class Tournament
{
    public string Id { get; set; }
    public GameItem Game { get; set; }
    public ConcurrentList<Player> Players { get; set; } = [];
    public ConcurrentList<ConcurrentList<Matchup>> Rounds { get; set; } = [];

    [JsonIgnore]
    public Player? Winner => Rounds.LastOrDefault()?.LastOrDefault()?.Winner;

    [JsonIgnore]
    public List<List<Matchup>> WinnersBracket
        => Rounds.Select(r => r.Where(m => m.Bracket == Bracket.Winners).ToList()).Where(r => r.Any()).ToList();

    [JsonIgnore]
    public List<List<Matchup>> LosersBracket
        => Rounds.Select(r => r.Where(m => m.Bracket == Bracket.Losers).ToList()).Where(r => r.Any()).ToList();

    public bool IsStarted => Rounds.Sum(r => r.Count) > 0;
    public bool IsComplete => Rounds.LastOrDefault()?.LastOrDefault()?.Winner is not null;
    public bool IsPending => Players.Any() && !IsComplete;
}

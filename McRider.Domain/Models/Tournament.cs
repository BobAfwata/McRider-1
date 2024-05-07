namespace McRider.Domain.Models;

public class Tournament
{
    public string Id { get; set; } = "tournament-" + Guid.NewGuid().ToString();
    public GameItem Game { get; set; }
    public ConcurrentList<Player> Players { get; set; } = [];
    public ConcurrentList<ConcurrentList<Matchup>> Rounds { get; set; } = [];

    [JsonIgnore]
    public IEnumerable<Matchup> Matchups => Rounds.SelectMany(r => r);

    [JsonIgnore]
    public List<List<Matchup>> WinnersBracket
        => Rounds.Select(r => r.Where(m => m.Bracket == Bracket.Winners).ToList()).Where(r => r.Any()).ToList();

    [JsonIgnore]
    public List<List<Matchup>> LosersBracket
        => Rounds.Select(r => r.Where(m => m.Bracket == Bracket.Losers).ToList()).Where(r => r.Any()).ToList();

    public bool IsStarted => Matchups.Where(m => m.HasPlayers).Any(m => m.IsPlayed);
    public bool IsPlayed => Matchups.Where(m => m.HasPlayers).All(m => m.IsPlayed);
    public bool IsPending => Players.Any() && !IsPlayed;
}

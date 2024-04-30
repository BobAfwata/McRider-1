namespace McRider.Domain.Models;

public class Tournament
{
    public string Id { get; set; }
    public GameItem Game { get; set; }
    public List<Player> Players { get; set; } = [];
    public List<List<Matchup>> Rounds { get; set; } = [];
    public bool IsStarted => Rounds.Sum(r => r.Count) > 0;
    public bool IsComplete => Rounds.LastOrDefault()?.LastOrDefault()?.Winner != null;
    public bool IsPending => Players.Any() && !IsComplete;
}

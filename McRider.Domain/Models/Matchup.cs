
namespace McRider.Domain.Models;


public class Matchup
{
    public static int Counter = 0;

    public string Id { get; set; } = "matchup-" + Guid.NewGuid().ToString();

    public int Index { get; set; } = Counter++;

    public Bracket Bracket { get; set; } = Bracket.Winners;

    public int Round { get; set; } = 1;
    public GameItem Game { get; set; }
    public ConcurrentList<MatchupEntry> Entries { get; set; } = [];

    public ConcurrentList<Matchup> ParentMatchups => Entries.Select(e => e.ParentMatchup).Where(x => x is not null).ToList();

    public bool IsComplete { get; set; } = false;

    public bool IsByeMatchup => Entries.Count == 1;

    public bool HasPlayers => Entries.Count(e => e.Player is not null) > 1;

    public bool HasWinner
    {
        get
        {
            if (IsComplete == false) return false;
            if (Entries.Count < 1) return false;
            if (Entries.Count == 1) return true;

            var ordered = Entries.Where(e => e.Player is not null).OrderBy(e => e);
            var first = ordered.FirstOrDefault();
            var firstTired = ordered.Where(e => e.CompareTo(first) == 0);
            return firstTired.Count() == 1;
        }
    }

    public IEnumerable<Player> Players => Entries.Select((e, i) => GetPlayerAt(i)).Where(x => x is not null).ToList();

    public Player? Player1 => GetPlayerAt(0);
    public Player? Player2 => GetPlayerAt(1);

    public Player? Winner
    {
        get
        {
            if (IsByeMatchup == true)
                return Entries.FirstOrDefault()?.Player;
            if (IsComplete == false)
                return null;

            var ordered = Entries.Where(e => e.Player is not null).OrderBy(e => e);
            var first = ordered.FirstOrDefault();
            var firstTired = ordered.Where(e => e.CompareTo(first) == 0).ToList();

            if (firstTired.Count != 1)
                return null;

            return firstTired.FirstOrDefault()?.Player;
        }
    }

    public Player? Loser
    {
        get
        {
            if (IsByeMatchup == true)
                return null;
            if (IsComplete == false)
                return null;

            var ordered = Entries.Where(e => e.Player is not null).OrderBy(e => e);
            var last = ordered.LastOrDefault();
            var lastTired = ordered.Where(e => e.CompareTo(last) == 0).ToList();

            if (lastTired.Count != 1)
                return null;

            return lastTired.LastOrDefault()?.Player;
        }
    }

    private Player? GetPlayerAt(int index)
    {
        var entry = Entries?.ElementAtOrDefault(index);
        if (Entries?.Count <= 1 && Round > 1)
            return null;

        if (entry is null)
            return null; // No Entry probably a bye

        if (entry.Player is not null)
            return entry.Player;

        var parentMatchup = entry.ParentMatchup;

        // Same Players in Set 2 GrandFinals as Set 1
        if (Bracket == Bracket.GrandFinals && parentMatchup?.Bracket == Bracket.GrandFinals)
        {
            if (parentMatchup.Entries.FirstOrDefault(e => e.IsWinner == true)?.ParentMatchup?.Bracket == Bracket.Winners)
                return null;

            return entry.Player = parentMatchup.GetPlayerAt(index);
        }

        if (Bracket == Bracket.Winners || Bracket == Bracket.GrandFinals)
            return entry.Player = parentMatchup?.Winner;

        // All loser bracket matchup must have a parentMatchup
        parentMatchup = parentMatchup ?? throw new InvalidOperationException("Parent Matchup is required for loser bracket.");
        
        if (parentMatchup.Bracket == Bracket.Winners)
            return entry.Player = parentMatchup.Loser;
        else
            return entry.Player = parentMatchup?.Winner;
    }

    public string PlayerVsPlayerText => $"{Player1?.Nickname} vs {Player2?.Nickname}";

    public override string ToString()
    {
        var label = Bracket == Bracket.Winners ? "W" : Bracket == Bracket.Losers ? "L" : "GF";
        return $"{label}{Round}M{Index}";
    }

    public void Reset()
    {
        foreach (var entry in Entries)
            entry?.Reset();
    }
}


public enum Bracket
{
    Winners = 1,
    Losers = 2,
    GrandFinals = 3
}
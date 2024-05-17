
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

    public bool IsPlayed { get; set; } = false;

    public bool IsByeMatchup
    {
        get
        {
            if (Players.Count() > 1)
                return false;

            if (Entries.Count() == 1)
                return true;

            if (ParentMatchups.All(p => p.IsByeMatchup))
                return true;

            // If this is a losers bracket match and
            if (Bracket == Bracket.Losers)
                // the one parent is a winners bracket match and
                if (ParentMatchups.Any(p => p.Bracket == Bracket.Winners))
                    // the other parent is a bye match
                    if (ParentMatchups.Any(p => p.IsByeMatchup))
                        return true;

            return false;
        }
    }

    public bool HasPlayers => Entries.Count(e => e.Player is not null) > 1;

    public bool HasWinner
    {
        get
        {
            if (IsPlayed == false) return false;
            if (Entries.Count < 1) return false;
            if (Entries.Count == 1) return true;

            var ordered = Entries.Where(e => e.Player is not null).OrderBy(e => e);
            var first = ordered.FirstOrDefault();
            var firstTired = ordered.Where(e => e.CompareTo(first) == 0);
            return firstTired.Count() == 1;
        }
    }

    public IEnumerable<Player> Players => Entries.Select((e, i) => GetPlayerAt(i)).Where(x => x is not null).ToList();

    public int PlayerCount => Players?.Count() ?? 0;

    public Player? Player1 => GetPlayerAt(0);
    public Player? Player2 => GetPlayerAt(1);

    public bool ExpectesPlayerEntry => Entries.Any(e => e.ExpectsPlayerEntry);

    public Player? Winner
    {
        get
        {
            if (ExpectesPlayerEntry == true)
                return null;

            if (PlayerCount == 1 || IsByeMatchup == true)
                return Entries.FirstOrDefault(e => e.Player != null)?.Player;

            if (IsPlayed == false)
                return null;

            var ordered = Entries.Where(e => e.Player is not null).OrderBy(e => e);
            var first = ordered.FirstOrDefault();
            var tired = ordered.Where(e => e.CompareTo(first) == 0).ToList();

            if (tired.Count != 1)
                return null;

            return tired.FirstOrDefault()?.Player;
        }
    }

    public Player? Loser
    {
        get
        {
            if (IsByeMatchup == true)
                return null;

            if (PlayerCount == 1 && ExpectesPlayerEntry != true)
                return null;

            if (IsPlayed == false)
                return null;

            var ordered = Entries.Where(e => e.Player is not null).OrderBy(e => e);
            var last = ordered.LastOrDefault();
            var lastTired = ordered.Where(e => e.CompareTo(last) == 0).ToList();

            if (lastTired.Count != 1)
                return null;

            return lastTired.LastOrDefault()?.Player;
        }
    }

    public Player? GetPlayerAt(int index)
    {
        if (Entries?.Count <= 1 && Round > 1)
            return null;

        var entry = Entries?.ElementAtOrDefault(index);

        // No Entry probably a bye
        if (entry is null)
            return null;

        return entry.Player;
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace McRider.Domain.Models;


public class Matchup
{
    public string? Id { get; set; } = "matchup-" + Guid.NewGuid().ToString();

    public int Round { get; set; } = 1;

    public GameItem Game { get; set; }

    public bool NeedsTireBreaker
    {
        get
        {
            var ordered = Entries.Where(e => e.Player != null).OrderBy(e => e);
            var first = ordered.FirstOrDefault();
            var firstTired = ordered.Where(e => e == first);
            return firstTired.Count() > 1;
        }
    }

    public Player? Player1 => Entries?.ElementAtOrDefault(0)?.Player ?? Entries?.ElementAtOrDefault(0)?.ParentMatchup?.Winner;
    public Player? Player2 => Entries?.ElementAtOrDefault(1)?.Player ?? Entries?.ElementAtOrDefault(1)?.ParentMatchup?.Winner;
    public Player? Winner
    {
        get
        {
            if (NeedsTireBreaker) return null;
            var ordered = Entries.Where(e => e.Player != null).OrderBy(e => e).ToList();
            return ordered.FirstOrDefault()?.Player;
        }
    }

    public List<MatchupEntry> Entries { get; set; } = [];

    public void Reset()
    {
        foreach (var entry in Entries)
            entry?.Reset();
    }
}

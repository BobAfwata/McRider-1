using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace McRider.MAUI.Extensions;

public static class TournamentExtensions
{
    public static void CreateRevealRounds(this Tournament tournament, bool randomize = true)
    {
        var revelImages = tournament.GetRevealImages();
        if (randomize)
            revelImages = revelImages.Randomize();
        
        tournament.Rounds[0] = []; // Clear the first round

        int index = 0;
        foreach(var image in revelImages)
        {
            var currentMatchup = new Matchup() { 
                Game = tournament.Game, 
                Index = ++index, 
                Bracket = Bracket.Winners,
                Metadata = { { "RevealImage", image } },
                Entries = []
            };

            var player = tournament.Players.ElementAtOrDefault(index % tournament.Players.Count);
            var entry = new MatchupEntry(currentMatchup);

            if(player is not null)
                entry.Player = player;

            currentMatchup.Entries.Add(entry);

            tournament.Rounds[0].Add(currentMatchup);
        }
    }

    public static IEnumerable<string> GetRevealImages(this Tournament tournament)
    {
        var images = tournament?.Game?.RevealImages ?? [];

        var assembly = Application.Current?.GetType().Assembly;
        var revealRegex = new Regex($"{App.Configs?.Theme ?? ""}(.?)reveal(.*).png");
        var matches = assembly?.GetManifestResourceNames().Where(r => revealRegex.IsMatch(r));

        if (matches?.Any() == true)
            images.AddRange(matches);

        return images.ToList();
    }

}

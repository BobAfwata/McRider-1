using McRider.Domain.Models;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using Font = System.Drawing.Font;

namespace McRider.Common.Extensions;

public static class TournamentVisualizationExtensions
{
    const float BOX_WIDTH = 100;
    const float BOX_HEIGHT = 40;
    const float WIDTH_SPACING_RATIO = 1.4F;
    const float HEIGHT_SPACING_RATIO = 2F;
    const float PADDING_H = 20;
    const float PADDING_V = PADDING_H * 2;
    const float TEXT_PADDING = 3;

    static readonly Font font = new Font("Arial", 10);

    public static Bitmap CreateTournamentImage(this Tournament tournament, bool showByes = true, bool showMatchId = true)
    {
        if (tournament?.Matchups?.Any() != true) return null;

        var height = tournament.Rounds.Max(r => r.Count) * BOX_HEIGHT * HEIGHT_SPACING_RATIO + PADDING_H * 3;
        var width = tournament.Rounds.Count * BOX_WIDTH * WIDTH_SPACING_RATIO + PADDING_H * 3;

        // Assume tournament is validated and contains required data.
        Bitmap bitmap = new Bitmap((int)width, (int)height); // Create a bitmap with some arbitrary dimensions.
        using Graphics g = Graphics.FromImage(bitmap);

        g.Clear(Color.White); // Clear the bitmap with a white background.
        var viewBox = new RectangleF(PADDING_H, PADDING_V, bitmap.Width - 2 * PADDING_H, (bitmap.Height - 2 * PADDING_V));

        tournament.FixParentMatchupRef();

        var hSpacing = (float)Math.Max(BOX_WIDTH * WIDTH_SPACING_RATIO, Math.Min(BOX_WIDTH * WIDTH_SPACING_RATIO, viewBox.Width / tournament.Rounds.Count));
        var vSpacing = (float)Math.Max(BOX_HEIGHT * HEIGHT_SPACING_RATIO, Math.Min(BOX_HEIGHT * HEIGHT_SPACING_RATIO, viewBox.Height / tournament.Rounds[0].Count));
        var possitions = new Dictionary<string, RectangleF>();

        // Draw round 1
        for (var match = 0; match < tournament.Rounds[0].Count; match++)
        {
            var matchup = tournament.Rounds[0][match];
            var pos = viewBox.Location + new SizeF(PADDING_H, vSpacing * match);
            var showVisual = showByes == true || matchup.IsByeMatchup != true || matchup.IsPlayed;
            var matchupRect = new RectangleF();

            for (var entry = 0; entry < matchup.Entries.Count; entry++)
            {
                var e = matchup.Entries[entry];
                var entryRect = GetEntryRectacle(e, pos + new SizeF(0F, entry * BOX_HEIGHT / 1.5F));

                possitions[e.Id] = entryRect;
                if (matchupRect.Location.IsEmpty)
                    matchupRect = new RectangleF(entryRect.Location, entryRect.Size);
                else
                    matchupRect.Size = new SizeF(matchupRect.Width, entryRect.Y - matchupRect.Y + entryRect.Height);

                if (showVisual)
                    DrawEntry(g, tournament, e, entryRect);
            }

            possitions[matchup.Id] = matchupRect;
        }

        // Draw remaining rounds
        for (var round = 1; round < tournament.Rounds.Count; round++)
        {
            var roundMatchups = tournament.Rounds[round];
            for (var match = 0; match < roundMatchups.Count; match++)
            {
                var matchup = roundMatchups[match];
                if(matchup.IsFinals())
                {
                    Console.WriteLine();
                }

                var parentRects = new List<RectangleF>();

                foreach (var p in matchup.ParentMatchups.Where(p => matchup.IsFinals() || p.Bracket == matchup.Bracket))
                    if (possitions.TryGetValue(p.Id, out var rect))
                        parentRects.Add(rect);

                if (parentRects.Any() != true)
                    continue;

                var pos = new PointF(
                    parentRects.Max(p => p.X) + hSpacing,
                    parentRects.Average(p => p.Y)
                );

                var showVisual = showByes == true || matchup.IsByeMatchup != true || matchup.IsPlayed;
                var matchupRect = new RectangleF();

                for (var entry = 0; entry < matchup.Entries.Count; entry++)
                {
                    var e = matchup.Entries[entry];
                    var entryRect = GetEntryRectacle(e, pos + new SizeF(0F, entry * (BOX_HEIGHT / 1.5F)));

                    possitions[e.Id] = entryRect;
                    if (matchupRect.Location.IsEmpty)
                        matchupRect = new RectangleF(entryRect.Location, entryRect.Size);
                    else
                        matchupRect.Size = new SizeF(matchupRect.Width, entryRect.Y - matchupRect.Y + entryRect.Height);

                    if (showVisual)
                        DrawEntry(g, tournament, e, entryRect);
                }

                possitions[matchup.Id] = matchupRect;
            }
        }

        // Draw joining lines
        foreach (var match in tournament.Matchups.Where(m => m.Round > 1))
        {
            // Skip if this is the first round
            if (match.Round <= 1)
                continue;

            // 
            foreach (var parent in match.ParentMatchups)
            {
                if (parent.Bracket == Bracket.Winners && match.Bracket == Bracket.Losers)
                    continue;

                foreach (var e in parent.Entries)
                {
                    var pE = match.Entries.FirstOrDefault(x => x.Player == e.Player);
                    if (possitions.TryGetValue(e?.Id ?? "", out var parentPos) != true)
                        continue;

                    if (possitions.TryGetValue(match.Id, out var matchPos))
                    {
                        var lineStart = matchPos.MidLeft();
                        var lineEnd = parentPos.MidRight();

                        var x = (1 - WIDTH_SPACING_RATIO) * BOX_WIDTH * .5F;

                        var turn1 = lineStart + new SizeF(x, 0);
                        var turn2 = new PointF(turn1.X, lineEnd.Y);

                        var points = new[] { lineStart, turn1, turn2, lineEnd };
                        g.DrawLines(Pens.Black, points);
                    }
                }
            }
        }

        // Draw winner placeholder
        var _pos = possitions.LastOrDefault().Value;
        var winPos = _pos.Location + new SizeF(0, hSpacing / 2);
        var winner = tournament.GetWinner();
        var winnerText = winner != null ? winner.Nickname : "Winner";

        g.DrawRectangle(Pens.Black, new RectangleF(winPos.X, winPos.Y, BOX_WIDTH, BOX_HEIGHT));
        g.DrawString(winnerText, new Font(font.FontFamily, font.Size * 1.8F), Brushes.Black, winPos + new SizeF(TEXT_PADDING * 2F, TEXT_PADDING * 2F));

        // Draw score board
        var sHeight = BOX_HEIGHT / 2;
        var scoresPos = new PointF(viewBox.Right - BOX_WIDTH * 1.5F, viewBox.Top - BOX_HEIGHT * .3F);
        g.DrawString("Score board", new Font(font.FontFamily, font.Size * 1.8F), Brushes.Black, scoresPos - new SizeF(PADDING_H, 0));

        // Draws scores for each player
        var players = tournament.Players.OrderByDescending(p => p.GetWins(tournament)).ToList();
        for (int i = 0; i < players.Count; i++)
        {
            var player = players[i];
            var score = player.GetScore(tournament);

            g.DrawString(player?.Nickname, font, Brushes.Black, new PointF(scoresPos.X, scoresPos.Y + BOX_HEIGHT + sHeight * i));
            g.DrawString(score.ToString().PadLeft(2, ' '), font, Brushes.Black, new PointF(scoresPos.X + BOX_WIDTH, scoresPos.Y + BOX_HEIGHT + sHeight * i));
        }

        return bitmap;
    }

    private static RectangleF GetEntryRectacle(MatchupEntry entry, PointF pos) => new RectangleF(pos.X, pos.Y, BOX_WIDTH, BOX_HEIGHT / 2);

    private static void DrawEntry(Graphics g, Tournament tournament, MatchupEntry entry, RectangleF rect)
    {
        var scorePosFactor = 6.5F / 10.0F;
        var pos = rect.Location;

        g.DrawRectangle(Pens.Black, rect);
        g.DrawLine(Pens.DarkGray, new PointF(pos.X + (rect.Width * scorePosFactor), pos.Y), new PointF(pos.X + (rect.Width * scorePosFactor), pos.Y + rect.Height));

        var brash = entry.IsWinner == true ? Brushes.Green : Brushes.Black;

        if (!string.IsNullOrEmpty(entry.Player?.Nickname))
            g.DrawString(entry.Player?.Nickname, font, brash, new PointF(pos.X + TEXT_PADDING, pos.Y + TEXT_PADDING));

        // Draw scores if available
        if (entry?.Distance > 0)
            g.DrawString(
                entry.Distance.ToString("0").PadLeft(4, ' '), font, brash,
                new PointF(pos.X + (BOX_WIDTH * scorePosFactor) + TEXT_PADDING, pos.Y + TEXT_PADDING)
            );
    }

}

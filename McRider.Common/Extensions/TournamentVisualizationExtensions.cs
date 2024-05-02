using System.Drawing;
using Font = System.Drawing.Font;

namespace McRider.Common.Extensions;

public static class TournamentVisualizationExtensions
{
    const float BOX_WIDTH = 100;
    const float BOX_HEIGHT = 40;
    const float PADDING = 20;
    const float TEXT_PADDING = 3;

    static readonly Font font = new Font("Arial", 10);

    public static Bitmap CreateTournamentImage(this Tournament tournament, bool showByes = true, bool showPlayers = true)
    {
        if (tournament is null) return null;

        var height = (tournament.Rounds.Max(r => r.Count) * 1.5 + 0.8) * (BOX_HEIGHT + PADDING);
        var width = (tournament.Rounds.Count + 1.7F) * (BOX_WIDTH + PADDING);

        // Assume tournament is validated and contains required data.
        Bitmap bitmap = new Bitmap((int)width, (int)height); // Create a bitmap with some arbitrary dimensions.
        using (Graphics g = Graphics.FromImage(bitmap))
        {
            g.Clear(Color.White); // Clear the bitmap with a white background.
            var viewBox = new RectangleF(PADDING, PADDING * 1.5F, bitmap.Width - 2 * PADDING, bitmap.Height - 3 * PADDING);

            // Draw winners bracket
            var rounds = tournament.FixParentMatchupRef().Rounds;
            var losersBraket = tournament.LosersBracket;

            var vOffsets = new Dictionary<int, float>();
            var vSpacings = new Dictionary<int, float>();
            var possitions = new Dictionary<string, PointF>();

            for (var round = 0; round < rounds.Count; round++)
            {
                var hSpacing = (float)Math.Max(BOX_WIDTH * 1.4, Math.Min(BOX_WIDTH * 1.4, viewBox.Width / rounds.Count));
                var vSpacing = (float)Math.Max(BOX_HEIGHT * 1.09, Math.Min(BOX_WIDTH * 1.4, viewBox.Height / rounds.ElementAt(round).Count));
                var vOffset = (vSpacing + viewBox.Height / rounds.ElementAt(round).Count) / 2 - (BOX_HEIGHT - PADDING) * 3;

                if (round > 0)
                {
                    vOffsets.TryGetValue(round - 1, out var lastVOffset);
                    vSpacings.TryGetValue(round - 1, out var lastVSpacing);

                    vOffset = lastVOffset + lastVSpacing * .5F;
                    vSpacing = Math.Min(viewBox.Height / 3, 2F * lastVSpacing);
                }

                vOffsets[round] = vOffset;
                vSpacings[round] = vSpacing;

                int match = 0;
                foreach (var matchup in rounds.ElementAt(round))
                {
                    var pos = viewBox.Location + new SizeF(hSpacing * round, vOffset + vSpacing * match);
                    var skipVisual = showByes == false && matchup.HasNoOpponent;

                    if (skipVisual == false)
                    {
                        possitions[matchup.Id] = pos;
                        DrawMathup(g, matchup, pos, showPlayers);
                        match++;
                    }
                    else if (matchup.Bracket == Bracket.Winners)
                    {
                        match++;
                    }
                }
            }

            // Draw winner placeholder
            var _pos = viewBox.Location + new SizeF(width - (BOX_WIDTH + PADDING * 1.5F), (viewBox.Height - BOX_HEIGHT * .8F) / 2F);
            g.DrawRectangle(Pens.Black, new RectangleF(_pos.X, _pos.Y, BOX_WIDTH, BOX_HEIGHT));

            possitions["Winner"] = _pos;

            var winner = tournament.Winner;
            var winnerText = winner != null ? winner.Nickname : "Winner";
            g.DrawString(winnerText, new Font(font.FontFamily, font.Size * 1.8F), Brushes.Black, new PointF(_pos.X + TEXT_PADDING * 2F, _pos.Y + TEXT_PADDING * 2F));

            // Draw joining lines
            foreach (var match in rounds.SelectMany(r => r).Where(m => m.Round > 1))
            {
                if (possitions.TryGetValue(match.Id, out var pos) != true) continue;
                foreach (var parent in match.ParentMatchups)
                {
                    if (possitions.TryGetValue(parent.Id, out var parentPos))
                        g.DrawLine(Pens.Black, new PointF(pos.X, pos.Y + BOX_HEIGHT / 2), new PointF(parentPos.X + BOX_WIDTH, parentPos.Y + BOX_HEIGHT / 2));
                }
            }
        }

        return bitmap;
    }

    private static void DrawMathup(Graphics g, Matchup matchup, PointF pos, bool showPlayers)
    {
        var scorePossFactor = 6.5F / 10.0F;
        //Draw a box for the matchup
        g.DrawRectangle(Pens.Black, new RectangleF(pos.X, pos.Y, BOX_WIDTH, BOX_HEIGHT));
        g.DrawLine(Pens.DarkGray, new PointF(pos.X + (BOX_WIDTH * scorePossFactor), pos.Y), new PointF(pos.X + (BOX_WIDTH * scorePossFactor), pos.Y + BOX_HEIGHT));
        g.DrawLine(Pens.DarkGray, new PointF(pos.X, pos.Y + BOX_HEIGHT / 2), new PointF(pos.X + BOX_WIDTH, pos.Y + BOX_HEIGHT / 2));

        // Draw the player names
        if (showPlayers)
        {
            if (!string.IsNullOrEmpty(matchup.Player1?.Nickname))
                g.DrawString(matchup.Player1?.Nickname, font, Brushes.Black, new PointF(pos.X + TEXT_PADDING, pos.Y + TEXT_PADDING));
            if (!string.IsNullOrEmpty(matchup.Player2?.Nickname))
                g.DrawString(matchup.Player2?.Nickname, font, Brushes.Black, new PointF(pos.X + TEXT_PADDING, pos.Y + BOX_HEIGHT / 2 + TEXT_PADDING));
        }
        else
        {
            g.DrawString(matchup.ToString(), font, Brushes.Black, new PointF(pos.X + TEXT_PADDING, pos.Y + TEXT_PADDING));
        }


        var entry1 = matchup.Entries.ElementAtOrDefault(0);
        var entry2 = matchup.Entries.ElementAtOrDefault(1);

        // Draw scores if available
        if (entry1?.Distance > 0)
            g.DrawString(
                entry1.Distance.ToString("0").PadLeft(4, ' '), font, entry1.IsWinner == true ? Brushes.Green : Brushes.Black,
                new PointF(pos.X + (BOX_WIDTH * scorePossFactor) + TEXT_PADDING, pos.Y + TEXT_PADDING)
            );

        if (entry2?.Distance > 0)
            g.DrawString(
                entry2.Distance.ToString("0").PadLeft(4, ' '), font, entry2.IsWinner == true ? Brushes.Green : Brushes.Black,
                new PointF(pos.X + (BOX_WIDTH * scorePossFactor) + TEXT_PADDING, pos.Y + BOX_HEIGHT / 2 + TEXT_PADDING)
            );
    }
}

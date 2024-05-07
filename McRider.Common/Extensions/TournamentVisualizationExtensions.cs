using System.Drawing;
using Font = System.Drawing.Font;

namespace McRider.Common.Extensions;

public static class TournamentVisualizationExtensions
{
    const float BOX_WIDTH = 100;
    const float BOX_HEIGHT = 40;
    const float WIDTH_SPACING_RATIO = 1.4F;
    const float HEIGHT_SPACING_RATIO = 2F;
    const float PADDING = 20;
    const float TEXT_PADDING = 3;

    static readonly Font font = new Font("Arial", 10);

    public static Bitmap CreateTournamentImage(this Tournament tournament, bool showByes = true, bool showPlayers = true)
    {
        if (tournament is null) return null;

        var height = tournament.Rounds.Max(r => r.Count) * BOX_HEIGHT * HEIGHT_SPACING_RATIO + PADDING * 3;
        var width = tournament.Rounds.Count * BOX_WIDTH * WIDTH_SPACING_RATIO + PADDING * 2;

        // Assume tournament is validated and contains required data.
        Bitmap bitmap = new Bitmap((int)width, (int)height); // Create a bitmap with some arbitrary dimensions.
        using (Graphics g = Graphics.FromImage(bitmap))
        {
            g.Clear(Color.White); // Clear the bitmap with a white background.
            var viewBox = new RectangleF(PADDING, PADDING * 1.5F, bitmap.Width - 2 * PADDING, bitmap.Height - 2 * PADDING);

            // Draw winners bracket
            var rounds = tournament.FixParentMatchupRef().Rounds;
            var losersBraket = tournament.LosersBracket;

            var vOffsets = new Dictionary<int, float>();
            var vSpacings = new Dictionary<int, float>();
            var possitions = new Dictionary<string, PointF>();

            var hSpacing = (float)Math.Max(BOX_WIDTH * WIDTH_SPACING_RATIO, Math.Min(BOX_WIDTH * WIDTH_SPACING_RATIO, viewBox.Width / rounds.Count));

            for (var round = 0; round < rounds.Count; round++)
            {
                var vSpacing = (float)Math.Max(BOX_HEIGHT * HEIGHT_SPACING_RATIO, Math.Min(BOX_HEIGHT * HEIGHT_SPACING_RATIO, viewBox.Height / rounds.ElementAt(round).Count));
                var vOffset = (vSpacing + viewBox.Height / rounds.ElementAt(round).Count) / 2 - (BOX_HEIGHT - PADDING) * 3;

                if (round > 0)
                {
                    vOffsets.TryGetValue(round - 1, out var lastVOffset);
                    vSpacings.TryGetValue(round - 1, out var lastVSpacing);

                    vOffset = lastVOffset + lastVSpacing * .5F;
                    vSpacing = Math.Min(viewBox.Height / 3, 2F * lastVSpacing);

                    var lastMatcup = rounds.ElementAt(round).LastOrDefault();
                    if (lastMatcup?.IsFinalsSet2() == true)
                    {
                        vOffset = lastVOffset;
                        vSpacing = lastVSpacing;
                        //hSpacing -= ((hSpacing - BOX_WIDTH) / 13);
                    }
                }

                vOffsets[round] = vOffset;
                vSpacings[round] = vSpacing;

                int match = 0;
                foreach (var matchup in rounds.ElementAt(round))
                {
                    var pos = viewBox.Location + new SizeF(hSpacing * round, vOffset + vSpacing * match);
                    var showVisual = showByes == true || matchup.IsByeMatchup != true || matchup.IsComplete;

                    if (showVisual)
                    {
                        if (matchup.IsFinalsSet2() && tournament.RequiresSet2Finals() != true)
                            continue;

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
            var _pos = possitions.LastOrDefault().Value;
            var winPos = _pos + new SizeF(0, hSpacing / 2);
            var winner = tournament.GetWinner();
            var winnerText = winner != null ? winner.Nickname : "Winner";

            g.DrawRectangle(Pens.Black, new RectangleF(winPos.X, winPos.Y, BOX_WIDTH, BOX_HEIGHT));
            g.DrawString(winnerText, new Font(font.FontFamily, font.Size * 1.8F), Brushes.Black, winPos + new SizeF(TEXT_PADDING * 2F, TEXT_PADDING * 2F));

            // Draw joining lines
            foreach (var match in tournament.Matchups.Where(m => m.Round > 1))
            {
                if (possitions.TryGetValue(match.Id, out var pos) != true) continue;
                foreach (var parent in match.ParentMatchups)
                {
                    if (possitions.TryGetValue(parent.Id, out var parentPos))
                    {
                        var lineEnd = new PointF(pos.X, pos.Y + BOX_HEIGHT / 2);
                        var lineStart = new PointF(parentPos.X + BOX_WIDTH, parentPos.Y + BOX_HEIGHT / 2);
                        var turn2 = lineEnd + new SizeF((1 - WIDTH_SPACING_RATIO) * BOX_WIDTH * .5F, 0);
                        var turn1 = new PointF(turn2.X, lineStart.Y);

                        var points = new[] { lineStart, turn1, turn2, lineEnd };
                        g.DrawLines(Pens.Black, points);
                    }
                }
            }

            var sHeight = BOX_HEIGHT / 2;
            var scoresPos = new PointF(viewBox.Right - BOX_WIDTH * 1.5F, viewBox.Top - BOX_HEIGHT * .3F);
            g.DrawString("Score board", new Font(font.FontFamily, font.Size * 1.8F), Brushes.Black, scoresPos - new SizeF(PADDING, 0));

            // Draws scores for each player
            var players = tournament.Players.OrderByDescending(p => p.GetWins(tournament)).ToList();
            for (int i = 0; i < players.Count; i++)
            {
                var player = players[i];
                var score = player.GetScore(tournament);

                g.DrawString(player?.Nickname, font, Brushes.Black, new PointF(scoresPos.X, scoresPos.Y + BOX_HEIGHT + sHeight * i));
                g.DrawString(score.ToString().PadLeft(2, ' '), font, Brushes.Black, new PointF(scoresPos.X + BOX_WIDTH, scoresPos.Y + BOX_HEIGHT + sHeight * i));
            }
        }

        return bitmap;
    }

    private static bool RequiresSet2Finals(this Tournament tournament)
    {
        var set2finals = tournament.Matchups.FirstOrDefault(x => x.IsFinalsSet2());
        if (set2finals?.IsComplete == true)
            return true;

        var set1finals = tournament.Matchups.FirstOrDefault(x => x.IsFinals());
        if (set2finals?.IsComplete != true)
            return true;

        if (set1finals.Loser?.GetScore(tournament) >= set1finals.Winner?.GetScore(tournament))
            return true;

        return false;
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

using McRider.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace McRider.MAUI.Extensions
{
    public static class TournamentExtensions
    {
        public static MatchupEntry? GetEntry(this Player player, Matchup matchup)
        {
            return matchup?.Entries.FirstOrDefault(e => e.Player?.Id == player?.Id);
        }

        public static Player GetWinner(this Matchup matchup)
        {
            matchup = matchup ?? throw new ArgumentNullException(nameof(matchup));
            if (matchup.Player1 == null && matchup.Player2 == null)
                return null;
            return GetWinner(matchup.Player1, matchup.Player2, matchup);
        }

        public static Player GetWinner(this Player player1, Player player2, Matchup matchup = null)
        {
            if (player1 == null && player2 == null)
                throw new ArgumentNullException("At least one player is required to determine the winner!");

            if (player1 != null && player2 == null)
                return player1; // Player 1 wins by default (Player 2 is null

            if (player1 == null && player2 != null)
                return player2; // Player 1 wins by default (Player 1 is null

            if (matchup == null)
                return null; // Game is not finished yet

            var percentageProgress = matchup?.GetPercentageProgress();
            if (percentageProgress < 100)
                return null; // Game is not finished yet
            try
            {
                var entryPlayer1 = matchup?.Entries.FirstOrDefault(e => e.Player?.Id == player1?.Id);
                var entryPlayer2 = matchup?.Entries.FirstOrDefault(e => e.Player?.Id == player2?.Id);

                if (entryPlayer1 > entryPlayer2)
                    return player1;
                else if (entryPlayer1 < entryPlayer2)
                    return player2;

                return null; // It's a tie
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static Player GetWinner(this Tournament tournament)
        {
            return tournament.Rounds.LastOrDefault()?.LastOrDefault().Winner;
        }

        public static double GetPercentageProgress(this MatchupEntry entry, GameItem game, bool? bestOfDistanceVsTime = true)
        {
            if (game == null) return 0;
            if (entry == null) return 0;

            var distanceProgress = game?.TargetDistance <= 0 ? 0 : entry.Distance / game?.TargetDistance;
            var timeProgress = game?.TargetTime?.TotalMicroseconds <= 0 ? 0 : entry.Time?.TotalMicroseconds / game?.TargetTime?.TotalMicroseconds;

            //App.Logger?.LogInformation($"{player.Nickname} Distance progress: {distanceProgress:0.00}, Time progress: {timeProgress:0.00}");

            if (bestOfDistanceVsTime == true)
                return Math.Round(Math.Min(100.0, Math.Max(distanceProgress ?? 0, timeProgress ?? 0) * 100), 2);

            if (bestOfDistanceVsTime == false)
                return Math.Round(Math.Min(100.0, (distanceProgress ?? 0) * 100), 2);

            return Math.Round(Math.Min(100.0, (timeProgress ?? 0) * 100), 2);
        }

        public static double GetPercentageProgress(this Player player, Matchup matchup, bool? bestOfDistanceVsTime = true)
        {
            if (player == null) return 0;
            if (matchup == null) return 0;

            var entry = player.GetEntry(matchup);
            return GetPercentageProgress(entry, matchup.Game, bestOfDistanceVsTime);
        }

        public static double GetPercentageProgress(this Matchup matchup) => GetPlayersProgress(matchup, true).Max();

        public static double GetPercentageTimeProgress(this Matchup matchup) => GetPlayersProgress(matchup, null).Max();

        public static double[] GetPlayersProgress(this Matchup matchup, bool? bestOfDistanceVsTime = true)
        {
            return matchup.Entries.Select(e => GetPercentageProgress(e, matchup.Game, bestOfDistanceVsTime)).ToArray();
        }
    }
}

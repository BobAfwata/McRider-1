namespace McRider.MAUI.Messages
{
    public class TournamentProgress
    {
        public TournamentProgress(Tournament tournament, Matchup matchup, double progress, string message = "")
        {
            Tournament = tournament;
            Matchup = matchup;
            Progress = progress;
            Message = message;
        }

        /// <summary>
        /// Progress Message
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Percentage Progress
        /// </summary>
        public double Progress { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Matchup Matchup { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Tournament Tournament { get; set; }
    }
}

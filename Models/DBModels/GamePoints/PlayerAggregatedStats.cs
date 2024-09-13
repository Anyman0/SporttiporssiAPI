using SporttiporssiAPI.Helpers;

namespace SporttiporssiAPI.Models.DBModels
{
    public class PlayerAggregatedStats
    {
        public int Points { get; set; }
        public int Assists { get; set; }
        public int Goals { get; set; }
        public int Shots { get; set; }
        public int PenaltyMinutes { get; set; }
        public int TimeOfIce { get; set; }
        public bool WinningGoal { get; set; }
        public int GamesPlayed { get; set; }
        public int CorsiFor { get; set; }
        public int CorsiAgainst { get; set; }
        public double Distance { get; set; }
        public int TotalPasses { get; set; }
        public int SuccessfulPasses { get; set; }
        public double ExpectedGoalsPlayer { get; set; }
        public double ExpectedGoalsTeam { get; set; }
        public double ExpectedGoalsAgainst { get; set; }
        public int Plus {  get; set; }
        public int Minus { get; set; }
        public int PlusMinus { get; set; }
        public int BlockedShots { get; set; }
        public int FaceoffsWon { get; set; }
        public int FaceoffsTotal { get; set; }
        public int Saves { get; set; }
        public HashSet<int> ProcessedGames { get; set; } = new HashSet<int>();
        public string TimeOnIceFormatted => TimeConversionHelper.ConvertSecondsToMinutesAndSeconds(AvgTimeOfIce);
        public int AvgTimeOfIce => GamesPlayed > 0 ? TimeOfIce / GamesPlayed : 0;
    }
}

namespace SporttiporssiAPI.Models.DBModels
{
    public class PeriodStats
    {
        public int Points { get; set; }
        public int Assists { get; set; }
        public int Goals { get; set; }
        public int Shots { get; set; }
        public int PenaltyMinutes { get; set; }
        public int TimeOfIce { get; set; }
        public int CorsiFor { get; set; }
        public int CorsiAgainst { get; set; }
        public double Distance { get; set; }
        public int TotalPasses { get; set; }
        public int SuccessfulPasses { get; set; }
        public double ExpectedGoalsPlayer { get; set; }
        public double ExpectedGoalsTeam { get; set; }
        public double ExpectedGoalsAgainst { get; set; }
        public int PlusMinus { get; set; }  // New
        public int BlockedShots { get; set; }  // New
        public int FaceoffsWon { get; set; }  // New
        public int FaceoffsTotal { get; set; }  // New
    }
}

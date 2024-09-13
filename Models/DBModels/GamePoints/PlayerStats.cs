namespace SporttiporssiAPI.Models.DBModels
{
    public class PlayerStats
    {
        public int JerseyId { get; set; }
        public int PlayerId { get; set; }
        public PeriodStats Period { get; set; }
        public double Distance { get; set; }
        public int TotalPasses { get; set; }
        public int SuccessfulPasses { get; set; }
        public double ExpectedGoalsPlayer { get; set; }
        public double ExpectedGoalsTeam { get; set; }
        public double ExpectedGoalsAgainst { get; set; }
    }
}

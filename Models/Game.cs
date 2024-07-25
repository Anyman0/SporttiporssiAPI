namespace SporttiporssiAPI.Models
{
    public class Game
    {
        public int GameId { get; set; } // Primary key
        public int Id { get; set; }
        public int Season { get; set; }
        public DateTime Start { get; set; }
        public Team HomeTeam { get; set; }
        public Team AwayTeam { get; set; }
        public string FinishedType { get; set; }
        public bool Started { get; set; }
        public bool Ended { get; set; }
        public string? BuyTicketsUrl { get; set; }
        public bool Stale { get; set; }
        public string Serie { get; set; }
        public DateTime? LastUpdated { get; set; }
    }

    public class Team
    {
        public string TeamId { get; set; }
        public string? TeamPlaceholder { get; set; }
        public string TeamName { get; set; }
        public int Goals { get; set; }
        public string? TimeOut { get; set; }
        public int PowerplayInstances { get; set; }
        public int PowerplayGoals { get; set; }
        public int ShortHandedInstances { get; set; }
        public int ShortHandedGoals { get; set; }
        public int? Ranking { get; set; }
        public DateTime GameStartDateTime { get; set; }
    }

}

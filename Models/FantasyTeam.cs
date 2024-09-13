namespace SporttiporssiAPI.Models
{
    public class FantasyTeam
    {
        public Guid FantasyTeamId { get; set; } // Primary key
        public string Teamname { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid Serie { get; set; }
        public int UserId { get; set; }
        public int TradesThisPhase { get; set; }
        public int FundsLeft { get; set; }
        public int TotalFTP {  get; set; }
       
    }
}

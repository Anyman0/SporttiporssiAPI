using System.ComponentModel.DataAnnotations.Schema;

namespace SporttiporssiAPI.Models
{
    public class Game
    {
        public int GameId { get; set; } // Primary key
        public int Id { get; set; }
        public int Season { get; set; }

        [NotMapped]
        public LiigaTeam HomeTeam { get; set; }
        [NotMapped]
        public LiigaTeam AwayTeam { get; set; }
        public DateTime Start { get; set; }
        public string HomeTeamName { get; set; }
        public int HomeTeamGoals { get; set; }
        public int AwayTeamGoals { get; set; }
        public string AwayTeamName { get; set; }
        public string FinishedType { get; set; }
        public bool Started { get; set; }
        public bool Ended { get; set; }
        public string Serie { get; set; }
        public DateTime? LastUpdated { get; set; }
        public Guid League {  get; set; }
    }

    public class LiigaTeam
    {       
        public string TeamId { get; set; }
        public string? TeamPlaceholder { get; set; }
        public string TeamName { get; set; }
        public int Goals { get; set; }
        public DateTime GameStartDateTime { get; set; }
    }

}

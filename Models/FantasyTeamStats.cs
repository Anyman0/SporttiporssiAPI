using System;
using System.ComponentModel.DataAnnotations;

namespace SporttiporssiAPI.Models
{
    public class FantasyTeamStats
    {
        [Key]
        public Guid FTPId { get; set; }

        [Required]
        public Guid FantasyTeamId { get; set; }

        public int TotalFTP { get; set; }

        public int Goals { get; set; }

        public int Assists { get; set; }

        public int Shots { get; set; }

        public int PenaltyMinutes { get; set; }

        public int FaceoffWins { get; set; }

        public int PlusMinus { get; set; }

        public int Saves { get; set; }

        public int BlockedShots { get; set; }

        public int Distance { get; set; }

    }
}

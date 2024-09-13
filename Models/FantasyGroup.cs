using Microsoft.EntityFrameworkCore;

namespace SporttiporssiAPI.Models
{
    public class FantasyGroup
    {
        public Guid GroupId { get; set; }
        public string GroupName { get; set; }
        public Guid Serie { get; set; }
        public int OffencePassFTP { get; set; }
        public int OffenceGoalFTP { get; set; }
        public int OffencePenaltyFTP { get; set; }
        public int OffencePenalty10FTP { get; set; }
        public int OffencePenalty20FTP { get; set; }
        public float OffenceShotFTP { get; set; }
        public float OffencePowerFTP { get; set; } // This is +/- statistics
        public int DefencePassFTP { get; set; }
        public int DefenceGoalFTP { get; set; }
        public int DefencePenaltyFTP { get; set; }
        public int DefencePenalty10FTP { get; set; }
        public int DefencePenalty20FTP { get; set; }
        public float DefenceShotFTP { get; set; }
        public float DefencePowerFTP { get; set; } // This is +/- statistics
        public int GoaliePassFTP { get; set; }
        public int GoalieGoalFTP { get; set; }
        public float GoalieSaveFTP { get; set; }
        public int GoalieWinFTP { get; set; }
        public int GoalieShutoutFTP { get; set; }
        public float FaceOffFTP { get; set; }
        public int BlockedShotFTP {  get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string GroupPasswordHash { get; set; }
        public string Salt { get; set; }
        public int TradesPerPhase { get; set; }
        public int StartMoney { get; set; }
    }
}

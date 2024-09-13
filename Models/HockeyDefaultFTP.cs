namespace SporttiporssiAPI.Models
{
    public class HockeyDefaultFTP
    {
        public int DefaultFTPId {  get; set; } 
        public Guid Serie { get; set; }
        public int TradesPerPhase { get; set; }
        public int OffencePassFTP { get; set; }
        public int OffenceGoalFTP { get; set; }
        public int OffencePenaltyFTP { get; set; }
        public int OffencePenalty10FTP { get; set; }
        public int OffencePenalty20FTP { get; set; }
        public double OffenceShotFTP { get; set; }
        public double OffencePowerFTP { get; set; }
        public int DefencePassFTP { get; set; }
        public int DefenceGoalFTP { get; set; }
        public int DefencePenaltyFTP { get; set; }
        public int DefencePenalty10FTP { get; set; }
        public int DefencePenalty20FTP { get; set; }
        public double DefenceShotFTP { get; set; }
        public double DefencePowerFTP { get; set; }
        public int GoaliePassFTP { get; set; }
        public int GoalieGoalFTP { get; set; }
        public double GoalieSaveFTP { get; set; }
        public int GoalieWinFTP { get; set; }
        public int? GoalieShutoutFTP { get; set; }
        public double FaceOffFTP { get; set; }
        public int BlockedShotFTP { get; set; }
        public int WinningGoalFTP { get; set; }
        public int StartMoney { get; set; }
    }
}

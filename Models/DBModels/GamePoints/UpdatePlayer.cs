namespace SporttiporssiAPI.Models.DBModels
{
    public class UpdatePlayer
    {      
        public int PlayerId { get; set; }
        public int Penalty2 { get; set; }
        public int Penalty10 { get; set; }
        public int Penalty20 { get; set; }
        public int Points { get; set; }
        public int Goals { get; set; }
        public int Assists { get; set; }
        public int Plus { get; set; }
        public int Minus { get; set; }
        public int PlusMinus { get; set; }
        public int PenaltyMinutes { get; set; }
        public int Shots { get; set; }
        public int FaceoffsTotal { get; set; }
        public int Saves { get; set; }
        public int GameWon { get; set; }
        public int GoalieShutout { get; set; }
        public int AllowedGoals { get; set; }
        public int TimeOnIce { get; set; }
        public int FaceoffsWon { get; set; }
        public int FaceoffsLost { get; set; }
        public decimal FaceoffWonPercentage { get; set; }
    }
}

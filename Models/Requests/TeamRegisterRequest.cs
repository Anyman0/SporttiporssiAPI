namespace SporttiporssiAPI.Models.Requests
{
    public class TeamRegisterRequest
    {
        public string Email { get; set; }
        public string Serie { get; set; }
        public string TeamName { get; set; }
        public int TradesThisPhase { get; set; }       
        public int FundsLeft { get; set; }
    }
}

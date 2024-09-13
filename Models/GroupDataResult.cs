namespace SporttiporssiAPI.Models
{
    public class GroupDataResult
    {
        public string GroupName { get; set; }
        public int? Standing { get; set; }
        public int? TeamsInGroup { get; set; }
        public Guid FantasyTeamId { get; set; }
        public int? TotalGroupFTP { get; set; }
        public int TradesPerPhase { get; set; }
    }
}

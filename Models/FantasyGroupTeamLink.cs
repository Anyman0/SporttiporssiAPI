namespace SporttiporssiAPI.Models
{
    public class FantasyGroupTeamLink
    {
        public int FantasyGroupTeamLinkId { get; set; } // primary key
        public Guid GroupId { get; set; }
        public Guid FantasyTeamId { get; set; }
        public int TotalGroupFTP { get; set; }
    }
}

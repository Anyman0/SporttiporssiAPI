namespace SporttiporssiAPI.Models
{
    public class FantasyTeamPlayerLink
    {
        public Guid FTPLinkId { get; set; } // Primary key
        public Guid FantasyTeamId { get; set; }
        public int PlayerId { get; set; }

    }
}

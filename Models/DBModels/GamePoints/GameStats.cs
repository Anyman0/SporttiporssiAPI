namespace SporttiporssiAPI.Models.DBModels
{
    public class GameStats
    {
        public List<TeamStats> AwayTeam { get; set; }
        public List<TeamStats> HomeTeam { get; set; }
    }
}

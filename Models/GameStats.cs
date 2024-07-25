namespace SporttiporssiAPI.Models
{
    public class GameStats
    {
        public GameDetails Game { get; set; }
        public List<Award> Awards { get; set; }
        public List<GamePlayer> HomeTeamPlayers { get; set; }
        public List<GamePlayer> AwayTeamPlayers { get; set; }
    }

    public class GameDetails
    {
        public int Id { get; set; }
        public int Season { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public GameTeam HomeTeam { get; set; }
        public GameTeam AwayTeam { get; set; }
        public List<Period> Periods { get; set; }
        public string FinishedType { get; set; }
        public bool Started { get; set; }
        public bool Ended { get; set; }
        public int GameTime { get; set; }
        public int Spectators { get; set; }
        public List<Referee> Referees { get; set; }
        public DateTime CacheUpdateDate { get; set; }
        public bool Stale { get; set; }
        public string Serie { get; set; }
    }

    public class GameTeam
    {
        public string TeamId { get; set; }
        public string TeamPlaceholder { get; set; }
        public string TeamName { get; set; }
        public int Goals { get; set; }
        public object TimeOut { get; set; }
        public List<GoalEvent> GoalEvents { get; set; }
        public List<PenaltyEvent> PenaltyEvents { get; set; }
        public List<GoalKeeperEvent> GoalKeeperEvents { get; set; }
        public List<GoalKeeperChange> GoalKeeperChanges { get; set; }
        public int PowerplayInstances { get; set; }
        public int PowerplayGoals { get; set; }
        public int ShortHandedInstances { get; set; }
        public int ShortHandedGoals { get; set; }
        public double ExpectedGoals { get; set; }
        public object Ranking { get; set; }
        public DateTime GameStartDateTime { get; set; }
    }

    public class GoalEvent
    {
        public int ScorerPlayerId { get; set; }
        public DateTime LogTime { get; set; }
        public bool WinningGoal { get; set; }
        public int GameTime { get; set; }
        public int Period { get; set; }
        public int EventId { get; set; }
        public List<string> GoalTypes { get; set; }
        public List<int> AssistantPlayerIds { get; set; }
        public string PlusPlayerIds { get; set; }
        public string MinusPlayerIds { get; set; }
        public int HomeTeamScore { get; set; }
        public int AwayTeamScore { get; set; }
        public Dictionary<int, int> AssistsSoFarInSeason { get; set; }
        public int GoalsSoFarInSeason { get; set; }
        public string VideoClipUrl { get; set; }
        public string VideoThumbnailUrl { get; set; }
    }

    public class PenaltyEvent
    {
        public int PlayerId { get; set; }
        public int SuffererPlayerId { get; set; }
        public DateTime LogTime { get; set; }
        public int GameTime { get; set; }
        public int Period { get; set; }
        public int PenaltyBegintime { get; set; }
        public int PenaltyEndtime { get; set; }
        public string PenaltyFaultName { get; set; }
        public string PenaltyFaultType { get; set; }
        public string PenaltyInfo { get; set; }
        public int PenaltyMinutes { get; set; }
    }

    public class GoalKeeperEvent
    {
        public int PlayerId { get; set; }
        public int BeginTime { get; set; }
        public int EmptyNet { get; set; }
        public int EndTime { get; set; }
        public DateTime LogTime { get; set; }
        public int GameTime { get; set; }
        public int Period { get; set; }
    }

    public class GoalKeeperChange
    {
        
    }


    public class Period
    {
        public int Index { get; set; }
        public int HomeTeamGoals { get; set; }
        public int AwayTeamGoals { get; set; }
        public string Category { get; set; }
        public int StartTime { get; set; }
        public int EndTime { get; set; }
    }

    public class Referee
    {
        public string FirstName { get; set; }
        public int Jersey { get; set; }
        public string LastName { get; set; }
        public int OfficialID { get; set; }
        public string RoleAbbrv { get; set; }
        public string RoleName { get; set; }
        public int SecondaryID { get; set; }
        public object TagA { get; set; }
        public object TagB { get; set; }
        public string PictureUrl { get; set; }
    }

    public class Award
    {
        public int Id { get; set; }
        public int AwardCategory { get; set; }
        public string AwardIssuer { get; set; }
        public string AwardName { get; set; }
        public int AwardPoint { get; set; }
        public int PlayerId { get; set; }
        public string TeamId { get; set; }
    }

    public class GamePlayer
    {
        public int Id { get; set; }
        public string TeamId { get; set; }
        public string TeamName { get; set; }
        public int? Line { get; set; }
        public string CountryOfBirth { get; set; }
        public string PlaceOfBirth { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Nationality { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
        public string RoleCode { get; set; }
        public string Handedness { get; set; }
        public int Height { get; set; }
        public int Weight { get; set; }
        public bool Captain { get; set; }
        public bool Rookie { get; set; }
        public bool AlternateCaptain { get; set; }
        public int Jersey { get; set; }
        public string PictureUrl { get; set; }
        public bool Injured { get; set; }
        public bool Suspended { get; set; }
        public bool Removed { get; set; }
        public List<object> Awards { get; set; } // If you have details about awards, update this type.
        public object Sponsors { get; set; }
        public object ExtraDescription { get; set; }
        public object ExtraStrengths { get; set; }
        public bool ExtraRookiePicture { get; set; }
    }

}

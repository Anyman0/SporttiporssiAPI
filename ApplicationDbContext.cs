using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SporttiporssiAPI.Models;
using SporttiporssiAPI.Models.DBModels;
using System.ComponentModel.DataAnnotations;
namespace SporttiporssiAPI
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Player> Players { get; set; }
        public DbSet<LiigaStanding> LiigaStandings { get; set; }
        public DbSet<LeagueStanding> LeagueStandings { get; set; }
        public DbSet<FantasyGroup> FantasyGroups { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Series> Series { get; set; }
        public DbSet<SportPhase> SportPhases { get; set; }
        public DbSet<FantasyGroupTeamLink> FantasyGroupTeamLinks { get; set; }
        public DbSet<FantasyTeam> FantasyTeams { get; set; }
        public DbSet<FantasyTeamPlayerLink> FantasyTeamPlayerLinks { get; set; }
        public DbSet<FantasyTeamStats> FantasyTeamStats { get; set; }
        public DbSet<GroupDataResult> GroupDataResults { get; set; }
        public DbSet<HockeyDefaultFTP> HockeyDefaultFTPs { get; set; }
        public DbSet<Models.DBModels.Stage> Stages { get; set; }
        public DbSet<Models.DBModels.Team> Teams { get; set; }
        public DbSet<Models.DBModels.Event> Events { get; set; }
        public DbSet<CanTradeResult> CanTradeResults { get; set; }


        public async Task<List<GroupDataResult>> GetGroupDataAndStandingByTeamId(Guid teamId)
        {
            var teamIdParam = new SqlParameter("@TeamId", teamId);
            var results = await GroupDataResults.FromSqlRaw("EXEC GetGroupDataAndStandingByTeamId @teamId", teamIdParam).ToListAsync();
            return results;
        }   


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LiigaStanding>()
            .ToTable("LiigaStanding")
            .HasKey(l => l.LiigaStandingId);

            modelBuilder.Entity<LeagueStanding>()
                .ToTable("LeagueStanding")
                .HasKey(l => l.Id);

            // Additional configuration if needed
            modelBuilder.Entity<LiigaStanding>()
                .Property(l => l.TeamId)
                .HasMaxLength(255);

            modelBuilder.Entity<LiigaStanding>()
                .Property(l => l.TeamName)
                .HasMaxLength(255);

            modelBuilder.Entity<LiigaStanding>()
                .Property(l => l.WinPercentage)
                .HasMaxLength(50);

            modelBuilder.Entity<LiigaStanding>()
                .Property(l => l.PowerPlayPercentage)
                .HasMaxLength(50);

            modelBuilder.Entity<LiigaStanding>()
                .Property(l => l.ShortHandedPercentage)
                .HasMaxLength(50);

            modelBuilder.Entity<Player>().ToTable("Player");
            modelBuilder.Entity<Player>().HasKey(p => p.Id);

            modelBuilder.Entity<Models.DBModels.Stage>().ToTable("Stages");
            modelBuilder.Entity<Models.DBModels.Stage>().HasKey(s => s.Id);
            modelBuilder.Entity<Models.DBModels.Team>().ToTable("Teams");
            modelBuilder.Entity<Models.DBModels.Team>().HasKey(t => t.Id);

            modelBuilder.Entity<Models.DBModels.Team>()
              .HasMany(t => t.HomeEvents)
              .WithOne(e => e.HomeTeam)
              .HasForeignKey(e => e.HomeTeamId)
              .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Models.DBModels.Team>()
                .HasMany(t => t.AwayEvents)
                .WithOne(e => e.AwayTeam)
                .HasForeignKey(e => e.AwayTeamId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Models.Game>().ToTable("Game");
            modelBuilder.Entity<Models.Game>().HasKey(g => g.GameId);

            modelBuilder.Entity<Models.DBModels.Event>().ToTable("Events");
            modelBuilder.Entity<Models.DBModels.Event>().HasKey(e => e.Id);

            modelBuilder.Entity<Models.DBModels.Event>()
                    .HasOne(e => e.Stage)
                    .WithMany(s => s.Events)
                    .HasForeignKey(e => e.StageId);

            modelBuilder.Entity<Models.DBModels.Event>()
                .HasOne(e => e.HomeTeam)
                .WithMany(t => t.HomeEvents)
                .HasForeignKey(e => e.HomeTeamId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Models.DBModels.Event>()
                .HasOne(e => e.AwayTeam)
                .WithMany(t => t.AwayEvents)
                .HasForeignKey(e => e.AwayTeamId)
                .OnDelete(DeleteBehavior.Restrict);           

            modelBuilder.Entity<HockeyDefaultFTP>().ToTable("HockeyDefaultFTP");
            modelBuilder.Entity<HockeyDefaultFTP>().HasKey(d => d.DefaultFTPId);

            modelBuilder.Entity<FantasyGroup>().ToTable("FantasyGroup");
            modelBuilder.Entity<FantasyGroup>().HasKey(g => g.GroupId);
            modelBuilder.Entity<FantasyGroup>()
                .Property(fg => fg.OffenceShotFTP)
                .HasColumnType("float");

            modelBuilder.Entity<FantasyGroup>()
                .Property(fg => fg.OffencePowerFTP)
                .HasColumnType("float");

            modelBuilder.Entity<FantasyGroup>()
                .Property(fg => fg.DefenceShotFTP)
                .HasColumnType("float");

            modelBuilder.Entity<FantasyGroup>()
                .Property(fg => fg.DefencePowerFTP)
                .HasColumnType("float");

            modelBuilder.Entity<FantasyGroup>()
                .Property(fg => fg.GoalieSaveFTP)
                .HasColumnType("float");

            modelBuilder.Entity<FantasyGroup>()
                .Property(fg => fg.FaceOffFTP)
                .HasColumnType("float");

            modelBuilder.Entity<FantasyTeam>().ToTable("FantasyTeam")
                .HasKey(ft => ft.FantasyTeamId);

            modelBuilder.Entity<GroupDataResult>().HasNoKey();
            modelBuilder.Entity<CanTradeResult>().HasNoKey();
            modelBuilder.Entity<LiigaTeam>().HasNoKey();

            modelBuilder.Entity<Series>().ToTable("Series");
            modelBuilder.Entity<Series>().HasKey(s => s.SerieId);

            modelBuilder.Entity<SportPhase>().ToTable("SportPhases");
            modelBuilder.Entity<SportPhase>().HasKey(s => s.SportPhaseId);

            modelBuilder.Entity<FantasyGroupTeamLink>().ToTable("FantasyGroupTeamLink")
                .HasKey(fg => fg.FantasyGroupTeamLinkId);
            modelBuilder.Entity<FantasyTeamPlayerLink>().ToTable("FantasyTeamPlayerLink")
                .HasKey(ft => ft.FTPLinkId);

            modelBuilder.Entity<Game>()
           .ToTable("Game")
           .HasKey(g => g.GameId);

           // modelBuilder.Entity<Game>()
           //.OwnsOne(g => g.HomeTeam, team =>
           //{
           //    team.Property(t => t.TeamId).HasColumnName("HomeTeamId");
           //    team.Property(t => t.TeamPlaceholder).HasColumnName("HomeTeamPlaceholder");
           //    team.Property(t => t.TeamName).HasColumnName("HomeTeamName");
           //    team.Property(t => t.Goals).HasColumnName("HomeTeamGoals");
           //    team.Property(t => t.TimeOut).HasColumnName("HomeTeamTimeOut");
           //    team.Property(t => t.PowerplayInstances).HasColumnName("HomeTeamPowerplayInstances");
           //    team.Property(t => t.PowerplayGoals).HasColumnName("HomeTeamPowerplayGoals");
           //    team.Property(t => t.ShortHandedInstances).HasColumnName("HomeTeamShortHandedInstances");
           //    team.Property(t => t.ShortHandedGoals).HasColumnName("HomeTeamShortHandedGoals");
           //    team.Property(t => t.Ranking).HasColumnName("HomeTeamRanking");
           //    team.Property(t => t.GameStartDateTime).HasColumnName("HomeTeamGameStartDateTime");
           //});

           // modelBuilder.Entity<Game>()
           //     .OwnsOne(g => g.AwayTeam, team =>
           //     {
           //         team.Property(t => t.TeamId).HasColumnName("AwayTeamId");
           //         team.Property(t => t.TeamPlaceholder).HasColumnName("AwayTeamPlaceholder");
           //         team.Property(t => t.TeamName).HasColumnName("AwayTeamName");
           //         team.Property(t => t.Goals).HasColumnName("AwayTeamGoals");
           //         team.Property(t => t.TimeOut).HasColumnName("AwayTeamTimeOut");
           //         team.Property(t => t.PowerplayInstances).HasColumnName("AwayTeamPowerplayInstances");
           //         team.Property(t => t.PowerplayGoals).HasColumnName("AwayTeamPowerplayGoals");
           //         team.Property(t => t.ShortHandedInstances).HasColumnName("AwayTeamShortHandedInstances");
           //         team.Property(t => t.ShortHandedGoals).HasColumnName("AwayTeamShortHandedGoals");
           //         team.Property(t => t.Ranking).HasColumnName("AwayTeamRanking");
           //         team.Property(t => t.GameStartDateTime).HasColumnName("AwayTeamGameStartDateTime");
           //     });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Salt).IsRequired().HasMaxLength(255);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.Role).HasDefaultValue("user").HasMaxLength(50);
            });
        }
    }
   
    public class Player
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public int TeamId { get; set; }
        public string? TeamName { get; set; }
        public string? TeamShortName { get; set; }
        public string? Role { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Nationality { get; set; }
        public string? Tournament { get; set; }
        public string? PictureUrl { get; set; }
        public string? PreviousTeamsForTournament { get; set; }
        public bool Injured { get; set; }
        public int Jersey { get; set; }
        public int LastSeason { get; set; }
        public bool Goalkeeper { get; set; }
        public int Games { get; set; }
        public int PlayedGames { get; set; }
        public bool Rookie { get; set; }
        public bool Suspended { get; set; }
        public bool Removed { get; set; }
        public int? TimeOnIce { get; set; }
        public bool Current { get; set; }
        public int? Goals { get; set; }
        public int? Assists { get; set; }
        public int? Points { get; set; }
        public int? Plus { get; set; }
        public int? Minus { get; set; }
        public int? PlusMinus { get; set; }
        public int? PenaltyMinutes { get; set; }
        public int? Penalty2 {  get; set; }
        public int? Penalty10 { get; set; }
        public int? Penalty20 { get; set; }
        public int? PowerplayGoals { get; set; }
        public int? PenaltykillGoals { get; set; }
        public int WinningGoals { get; set; }
        public int? Shots { get; set; }
        public int? Saves { get; set; }
        public int? GoalieShutout { get; set; }
        public int? AllowedGoals { get; set; }
        public int? ShotsIntoGoal { get; set; }
        public int? FaceoffsWon { get; set; }
        public int? FaceoffsLost { get; set; }
        public int? ExpectedGoals { get; set; }
        public string? TimeOnIceAvg { get; set; }
        public double? FaceoffWonPercentage { get; set; }
        public double? ShotPercentage { get; set; }
        public int? FaceoffsTotal { get; set; }
        public DateTime LastUpdated { get; set; }
        public int? FTP {  get; set; }
        public int? Price { get; set; }
        public int? PlayerOwned { get; set; }
        public int? BlockedShots { get; set; }
        public int? GameWon { get; set; }
    }

    public class Group
    {
        [JsonProperty("group")]
        public List<FantasyGroup> FantasyGroups { get; set; }
    }

    public class LiigaStandingResponse
    {
        [JsonProperty("season")]
        public List<LiigaStanding> SeasonStandings { get; set; }
    }

    public class LiigaStanding
    {
        public int LiigaStandingId { get; set; } // Primary Key, auto-incremented
        public int InternalId { get; set; }
        public string? TeamId { get; set; } // NVARCHAR(255)
        public string? TeamName { get; set; } // NVARCHAR(255), Nullable
        public int Ranking { get; set; }
        public int Games { get; set; }
        public int Wins { get; set; }
        public string? WinPercentage { get; set; } // NVARCHAR(10)
        public int OvertimeWins { get; set; }
        public int Losses { get; set; }
        public int OvertimeLosses { get; set; }
        public int Ties { get; set; }
        public int Points { get; set; }
        public int Goals { get; set; }
        public int GoalsAgainst { get; set; }
        public string? PowerPlayPercentage { get; set; } // NVARCHAR(10)
        public int PowerPlayInstances { get; set; }
        public int PowerPlayTime { get; set; }
        public int PowerPlayGoals { get; set; }
        public string? ShortHandedPercentage { get; set; } // NVARCHAR(10)
        public int ShortHandedInstances { get; set; }
        public int ShortHandedTime { get; set; }
        public int ShortHandedGoalsAgainst { get; set; }
        public int PenaltyMinutes { get; set; }
        public int TwoMinutePenalties { get; set; }
        public int FiveMinutePenalties { get; set; }
        public int TenMinutePenalties { get; set; }
        public int TwentyMinutePenalties { get; set; }
        public int TwentyFiveMinutePenalties { get; set; }
        public int TotalPenalties { get; set; }
        public int LiveRanking { get; set; }
        public int LiveGames { get; set; }
        public int LiveWins { get; set; }
        public int LiveLosses { get; set; }
        public int LiveTies { get; set; }
        public int LivePoints { get; set; }
        public double Distance { get; set; } // FLOAT
        public double DistancePerGame { get; set; } // FLOAT
        public double? PointsPerGame { get; set; } // FLOAT
        public DateTime? LastUpdated { get; set; } // Nullable DateTime
    }

    public class Series
    {
        public Guid SerieId { get; set; }
        public string SerieName { get; set; }
    }    

}

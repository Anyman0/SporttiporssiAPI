using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SporttiporssiAPI.Models;
using System.ComponentModel.DataAnnotations;
namespace SporttiporssiAPI
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Player> Players { get; set; }
        public DbSet<LiigaStanding> LiigaStandings { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LiigaStanding>()
            .ToTable("LiigaStanding")
            .HasKey(l => l.LiigaStandingId);

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

            modelBuilder.Entity<Game>()
           .ToTable("Game")
           .HasKey(g => g.GameId);

            modelBuilder.Entity<Game>()
           .OwnsOne(g => g.HomeTeam, team =>
           {
               team.Property(t => t.TeamId).HasColumnName("HomeTeamId");
               team.Property(t => t.TeamPlaceholder).HasColumnName("HomeTeamPlaceholder");
               team.Property(t => t.TeamName).HasColumnName("HomeTeamName");
               team.Property(t => t.Goals).HasColumnName("HomeTeamGoals");
               team.Property(t => t.TimeOut).HasColumnName("HomeTeamTimeOut");
               team.Property(t => t.PowerplayInstances).HasColumnName("HomeTeamPowerplayInstances");
               team.Property(t => t.PowerplayGoals).HasColumnName("HomeTeamPowerplayGoals");
               team.Property(t => t.ShortHandedInstances).HasColumnName("HomeTeamShortHandedInstances");
               team.Property(t => t.ShortHandedGoals).HasColumnName("HomeTeamShortHandedGoals");
               team.Property(t => t.Ranking).HasColumnName("HomeTeamRanking");
               team.Property(t => t.GameStartDateTime).HasColumnName("HomeTeamGameStartDateTime");
           });

            modelBuilder.Entity<Game>()
                .OwnsOne(g => g.AwayTeam, team =>
                {
                    team.Property(t => t.TeamId).HasColumnName("AwayTeamId");
                    team.Property(t => t.TeamPlaceholder).HasColumnName("AwayTeamPlaceholder");
                    team.Property(t => t.TeamName).HasColumnName("AwayTeamName");
                    team.Property(t => t.Goals).HasColumnName("AwayTeamGoals");
                    team.Property(t => t.TimeOut).HasColumnName("AwayTeamTimeOut");
                    team.Property(t => t.PowerplayInstances).HasColumnName("AwayTeamPowerplayInstances");
                    team.Property(t => t.PowerplayGoals).HasColumnName("AwayTeamPowerplayGoals");
                    team.Property(t => t.ShortHandedInstances).HasColumnName("AwayTeamShortHandedInstances");
                    team.Property(t => t.ShortHandedGoals).HasColumnName("AwayTeamShortHandedGoals");
                    team.Property(t => t.Ranking).HasColumnName("AwayTeamRanking");
                    team.Property(t => t.GameStartDateTime).HasColumnName("AwayTeamGameStartDateTime");
                });

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
        public string? TimeOnIce { get; set; }
        public bool Current { get; set; }
        public int? Goals { get; set; }
        public int? Assists { get; set; }
        public int? Points { get; set; }
        public int? Plus { get; set; }
        public int? Minus { get; set; }
        public int? PlusMinus { get; set; }
        public int? PenaltyMinutes { get; set; }
        public int? PowerplayGoals { get; set; }
        public int? PenaltykillGoals { get; set; }
        public int WinningGoals { get; set; }
        public int? Shots { get; set; }
        public int? ShotsIntoGoal { get; set; }
        public int? FaceoffsWon { get; set; }
        public int? FaceoffsLost { get; set; }
        public int? ExpectedGoals { get; set; }
        public string? TimeOnIceAvg { get; set; }
        public double? FaceoffWonPercentage { get; set; }
        public double? ShotPercentage { get; set; }
        public int? FaceoffsTotal { get; set; }
        public DateTime LastUpdated { get; set; }
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

}

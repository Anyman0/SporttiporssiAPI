using Newtonsoft.Json;

namespace SporttiporssiAPI.Models.DBModels
{
    public class LeagueStanding
    {
        public Guid Id { get; set; } // Primary Key, can be auto-generated
        public Guid SerieId { get; set; }

        [JsonProperty("Rank")]
        public int Rank { get; set; } // Ranking of the team

        [JsonProperty("Played")]
        public int Played { get; set; } // Total games played

        [JsonProperty("TeamName")]
        public string TeamName { get; set; } // Name of the team

        [JsonProperty("Points")]
        public int Points { get; set; } // Points the team has

        [JsonProperty("Wins")]
        public int Wins { get; set; } // Number of wins

        [JsonProperty("Losses")]
        public int Losses { get; set; } // Number of losses

        [JsonProperty("GoalsFor")]
        public int GoalsFor { get; set; } // Goals scored by the team

        [JsonProperty("GoalsAgainst")]
        public int GoalsAgainst { get; set; } // Goals conceded by the team

        [JsonProperty("GoalDifference")]
        public int GoalDifference { get; set; } // Difference between GoalsFor and GoalsAgainst

        public DateTime LastUpdated { get; set; } 
    }
}

using Newtonsoft.Json;

namespace SporttiporssiAPI.Models
{
    public class LeagueStandingsResponse
    {
        [JsonProperty("Sid")]
        public string Sid { get; set; }

        [JsonProperty("Snm")]
        public string Snm { get; set; }

        [JsonProperty("Scd")]
        public string Scd { get; set; }

        [JsonProperty("Cid")]
        public string Cid { get; set; }

        [JsonProperty("Cnm")]
        public string Cnm { get; set; }

        [JsonProperty("CnmT")]
        public string CnmT { get; set; }

        [JsonProperty("Csnm")]
        public string Csnm { get; set; }

        [JsonProperty("Ccd")]
        public string Ccd { get; set; }

        [JsonProperty("Scu")]
        public int Scu { get; set; }

        [JsonProperty("Sds")]
        public string Sds { get; set; }

        [JsonProperty("Chi")]
        public int Chi { get; set; }

        [JsonProperty("Shi")]
        public int Shi { get; set; }

        [JsonProperty("Ccdiso")]
        public string Ccdiso { get; set; }

        [JsonProperty("Sdn")]
        public string Sdn { get; set; }

        [JsonProperty("LeagueTable")]
        public LeagueTable LeagueTable { get; set; }
    }

    // Model for the LeagueTable object
    public class LeagueTable
    {
        [JsonProperty("L")]
        public List<League> Leagues { get; set; }
    }

    // Model for the League object
    public class League
    {
        [JsonProperty("Tables")]
        public List<Table> Tables { get; set; }
    }

    // Model for the Table object
    public class Table
    {
        [JsonProperty("LTT")]
        public int Ltt { get; set; }

        [JsonProperty("team")]
        public List<Team> Teams { get; set; }
    } 
    

}

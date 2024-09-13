using Newtonsoft.Json;

namespace SporttiporssiAPI.Models
{
    public class LSGame
    {
        [JsonProperty("Stages")]
        public List<Stage> Stages { get; set; }
    }
    public class Stage
    {
        [JsonProperty("Sid")]
        public string Sid { get; set; }

        [JsonProperty("Snm")]
        public string Snm { get; set; }

        [JsonProperty("Cid")]
        public string Cid { get; set; }

        [JsonProperty("Scd")]
        public string Scd { get; set; }

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

        [JsonProperty("Events")]
        public List<Event> Events { get; set; }
    }

    public class Event
    {
        [JsonProperty("Eid")]
        public string Eid { get; set; }

        [JsonProperty("Pids")]
        public Dictionary<string, string> Pids { get; set; }

        [JsonProperty("Sids")]
        public Dictionary<string, string> Sids { get; set; }

        [JsonProperty("T1")]
        public List<Team> T1 { get; set; }

        [JsonProperty("T2")]
        public List<Team> T2 { get; set; }

        [JsonProperty("Eps")]
        public string Eps { get; set; }

        [JsonProperty("Esid")]
        public int Esid { get; set; }

        [JsonProperty("Epr")]
        public int Epr { get; set; }

        [JsonProperty("Ecov")]
        public int Ecov { get; set; }

        [JsonProperty("ErnInf")]
        public string ErnInf { get; set; }

        [JsonProperty("Et")]
        public int Et { get; set; }

        [JsonProperty("Esd")]
        public long Esd { get; set; } // Use long for timestamps

        [JsonProperty("EO")]
        public int EO { get; set; }

        [JsonProperty("EOX")]
        public int EOX { get; set; }

        [JsonProperty("Ehid")]
        public int Ehid { get; set; }

        [JsonProperty("Spid")]
        public int Spid { get; set; }

        [JsonProperty("Pid")]
        public int Pid { get; set; }
    }
    public class Team
    {
        [JsonProperty("Nm")]
        public string Nm { get; set; }

        [JsonProperty("ID")]
        public string ID { get; set; }

        [JsonProperty("Img")]
        public string Img { get; set; }

        [JsonProperty("Abr")]
        public string Abr { get; set; }

        [JsonProperty("Tnm")]
        public string Tnm { get; set; }

        [JsonProperty("tbd")]
        public int Tbd { get; set; }

        [JsonProperty("Gd")]
        public int Gd { get; set; }

        [JsonProperty("Pids")]
        public Dictionary<string, List<string>> Pids { get; set; }

        [JsonProperty("CoNm")]
        public string CoNm { get; set; }

        [JsonProperty("CoId")]
        public string CoId { get; set; }

        [JsonProperty("HasVideo")]
        public bool HasVideo { get; set; }
        // Additional fields from the new JSON response
        [JsonProperty("rnk")]
        public int Rnk { get; set; }

        [JsonProperty("Tid")]
        public string Tid { get; set; }

        [JsonProperty("win")]
        public int Win { get; set; }

        [JsonProperty("winn")]
        public string Winn { get; set; }

        [JsonProperty("wreg")]
        public int Wreg { get; set; }

        [JsonProperty("wap")]
        public int Wap { get; set; }

        [JsonProperty("pf")]
        public int Pf { get; set; }

        [JsonProperty("pa")]
        public int Pa { get; set; }

        [JsonProperty("wot")]
        public int Wot { get; set; }

        [JsonProperty("lst")]
        public int Lst { get; set; }

        [JsonProperty("lstn")]
        public string Lstn { get; set; }

        [JsonProperty("lreg")]
        public int Lreg { get; set; }

        [JsonProperty("lot")]
        public int Lot { get; set; }

        [JsonProperty("lap")]
        public int Lap { get; set; }

        [JsonProperty("drw")]
        public int Drw { get; set; }

        [JsonProperty("drwn")]
        public string Drwn { get; set; }

        [JsonProperty("gf")]
        public int Gf { get; set; }

        [JsonProperty("ga")]
        public int Ga { get; set; }

        [JsonProperty("ptsn")]
        public string Ptsn { get; set; }

        [JsonProperty("phr")]
        public List<int> Phr { get; set; }

        [JsonProperty("Ipr")]
        public int Ipr { get; set; }

        [JsonProperty("pts")]
        public int Pts { get; set; }

        [JsonProperty("pld")]
        public int Pld { get; set; }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SporttiporssiAPI.Models;
using SporttiporssiAPI.Models.DBModels;

namespace SporttiporssiAPI.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    public class LiigaStandingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly string liigaBaseAddress = "https://www.liiga.fi/api/v2/";
        private readonly string liveScoreBaseAddress = "https://livescore6.p.rapidapi.com/";
        private readonly string _rapidKey;
        private readonly string _rapidHost;

        public LiigaStandingController(ApplicationDbContext context, HttpClient httpClient, IConfiguration configuration)
        {
            _context = context;
            _httpClient = httpClient;
            _rapidKey = configuration["RapidAPI:Key"];
            _rapidHost = configuration["RapidAPI:Host"];
        }      

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LeagueStanding>>> GetStandings(string league)
        {
            var serie = await _context.Series.Where(s => s.SerieName == league).FirstOrDefaultAsync();
            return await _context.LeagueStandings.Where(s => s.SerieId == serie.SerieId).OrderBy(s => s.Rank).ToListAsync();
        }
      
        [AllowAnonymous]
        [HttpPost("PopulateLeagueStandings")]
        public async Task<ActionResult> PopulateLeagueStandings(string league)
        {
            var apiUrl = $"{liveScoreBaseAddress}leagues/v2/get-table?Category=hockey&Ccd=finland&Scd={league}";
            try
            {
                var serie = _context.Series.Where(s => s.SerieName == league).FirstOrDefault();
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(apiUrl),
                    Headers =
                    {
                        { "x-rapidapi-key", _rapidKey },
                        { "x-rapidapi-host", _rapidHost },
                    },
                };
                using (var response = await _httpClient.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();     
                    var leagueStandings = JsonConvert.DeserializeObject<LeagueStandingsResponse>(body);

                    if(leagueStandings.LeagueTable.Leagues != null)
                    {
                        var leagueFromTable = leagueStandings.LeagueTable.Leagues.FirstOrDefault();
                        var teams = leagueFromTable.Tables.FirstOrDefault().Teams.ToList();
                        foreach (var team in teams)
                        {
                            var existingTeam = await _context.LeagueStandings.Where(t => t.TeamName == team.Tnm).FirstOrDefaultAsync();
                            if (existingTeam != null)
                            {
                                _context.Entry(existingTeam).CurrentValues.SetValues(new
                                {
                                    Rank = team.Rnk,
                                    Played = team.Pld,
                                    TeamName = team.Tnm,
                                    Points = team.Pts,
                                    Wins = team.Win,
                                    Losses = team.Lst,
                                    GoalsFor = team.Gf,
                                    GoalsAgainst = team.Ga,
                                    GoalDifference = team.Gd,
                                    LastUpdated = DateTime.UtcNow
                                });
                            }
                            else
                            {
                                var teamStanding = new LeagueStanding
                                {
                                    SerieId = serie.SerieId,
                                    Rank = team.Rnk,
                                    Played = team.Pld,
                                    TeamName = team.Tnm,
                                    Points = team.Pts,
                                    Wins = team.Win,
                                    Losses = team.Lst,
                                    GoalsFor = team.Gf,
                                    GoalsAgainst = team.Ga,
                                    GoalDifference = team.Gd,
                                    LastUpdated = DateTime.UtcNow
                                };
                                await _context.LeagueStandings.AddAsync(teamStanding);
                            }
                        }
                    }
                }
                await _context.SaveChangesAsync();
                return Ok("Standings data populated successfully");
            }
            catch
            {
                return NotFound();
            }
        }

        [AllowAnonymous]
        [HttpPost("PopulateNHLStandings")]
        public async Task<ActionResult> PopulateNHLStandings(string league)
        {
            var apiUrl = $"{liveScoreBaseAddress}leagues/v2/get-table?Category=hockey&Ccd={league}&Scd={league}-regular-season";
            try
            {
                var serie = _context.Series.Where(s => s.SerieName == league).FirstOrDefault();
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(apiUrl),
                    Headers =
                    {
                        { "x-rapidapi-key", _rapidKey },
                        { "x-rapidapi-host", _rapidHost },
                    },
                };
                using (var response = await _httpClient.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();
                    var leagueStandings = JsonConvert.DeserializeObject<LeagueStandingsResponse>(body);

                    if (leagueStandings.LeagueTable.Leagues != null)
                    {
                        var leagueFromTable = leagueStandings.LeagueTable.Leagues.FirstOrDefault();
                        var teams = leagueFromTable.Tables.FirstOrDefault().Teams.ToList();
                        foreach (var team in teams)
                        {
                            var existingTeam = await _context.LeagueStandings.Where(t => t.TeamName == team.Tnm).FirstOrDefaultAsync();
                            if (existingTeam != null)
                            {
                                _context.Entry(existingTeam).CurrentValues.SetValues(new
                                {
                                    Rank = team.Rnk,
                                    Played = team.Pld,
                                    TeamName = team.Tnm,
                                    Points = team.Pts,
                                    Wins = team.Win,
                                    Losses = team.Lst,
                                    GoalsFor = team.Gf,
                                    GoalsAgainst = team.Ga,
                                    GoalDifference = team.Gd,
                                    LastUpdated = DateTime.UtcNow
                                });
                            }
                            else
                            {
                                var teamStanding = new LeagueStanding
                                {
                                    SerieId = serie.SerieId,
                                    Rank = team.Rnk,
                                    Played = team.Pld,
                                    TeamName = team.Tnm,
                                    Points = team.Pts,
                                    Wins = team.Win,
                                    Losses = team.Lst,
                                    GoalsFor = team.Gf,
                                    GoalsAgainst = team.Ga,
                                    GoalDifference = team.Gd,
                                    LastUpdated = DateTime.UtcNow
                                };
                                await _context.LeagueStandings.AddAsync(teamStanding);
                            }
                        }
                    }
                }
                await _context.SaveChangesAsync();
                return Ok("Standings data populated successfully");
            }
            catch
            {
                return NotFound();
            }
        }

        [HttpGet("GetTeamRank")]
        public async Task<int> GetRankingByTeamAndSerie(string teamName, string league)
        {
            var serie = await _context.Series.Where(s => s.SerieName == league).FirstOrDefaultAsync();
            return await _context.LeagueStandings.Where(s => s.TeamName == teamName && s.SerieId == serie.SerieId).Select(s => s.Rank).FirstOrDefaultAsync();
        }
    }
}

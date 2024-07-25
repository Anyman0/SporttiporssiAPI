using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace SporttiporssiAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class LiigaStandingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly string liigaBaseAddress = "https://www.liiga.fi/api/v2/";

        public LiigaStandingController(ApplicationDbContext context, HttpClient httpClient)
        {
            _context = context;
            _httpClient = httpClient;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LiigaStanding>>> GetStandings()
        {
            return await _context.LiigaStandings.OrderBy(s => s.Ranking).ToListAsync();
        }

        [HttpPost("{year}" ,Name = "PopulateLiigaStandings")]
        private async Task<ActionResult> PopulateLiigaStandings(int year)
        {
            var apiUrl = liigaBaseAddress + $"standings/?season={year}";
            var response = await _httpClient.GetStringAsync(apiUrl);

            var standings = JsonConvert.DeserializeObject<LiigaStandingResponse>(response);

            if (standings?.SeasonStandings != null)
            {
                foreach (var standing in standings.SeasonStandings)
                {
                    var existingStanding = await _context.LiigaStandings.FirstOrDefaultAsync(p => p.InternalId == standing.InternalId);
                    var teamName = ExtractAndFormatTeamName(standing.TeamId);
                    // Round percentages and points per game to two decimal places
                    standing.WinPercentage = FormatPercentage(standing.WinPercentage);
                    standing.PowerPlayPercentage = FormatPercentage(standing.PowerPlayPercentage);
                    standing.ShortHandedPercentage = FormatPercentage(standing.ShortHandedPercentage);
                    standing.PointsPerGame = standing.PointsPerGame.HasValue ? Math.Round(standing.PointsPerGame.Value, 2) : (double?)null;
                    if (existingStanding != null)
                    {
                        // Update existing record
                        // Ensure only non-key properties are updated
                        existingStanding.TeamName = teamName;
                        _context.Entry(existingStanding).CurrentValues.SetValues(new
                        {
                            standing.TeamId, 
                            existingStanding.TeamName,
                            standing.Ranking,
                            standing.Games,
                            standing.Wins,
                            standing.WinPercentage,
                            standing.OvertimeWins,
                            standing.Losses,
                            standing.OvertimeLosses,
                            standing.Ties,
                            standing.Points,
                            standing.Goals,
                            standing.GoalsAgainst,
                            standing.PowerPlayPercentage,
                            standing.PowerPlayInstances,
                            standing.PowerPlayTime,
                            standing.PowerPlayGoals,
                            standing.ShortHandedPercentage,
                            standing.ShortHandedInstances,
                            standing.ShortHandedTime,
                            standing.ShortHandedGoalsAgainst,
                            standing.PenaltyMinutes,
                            standing.TwoMinutePenalties,
                            standing.FiveMinutePenalties,
                            standing.TenMinutePenalties,
                            standing.TwentyMinutePenalties,
                            standing.TwentyFiveMinutePenalties,
                            standing.TotalPenalties,
                            standing.LiveRanking,
                            standing.LiveGames,
                            standing.LiveWins,
                            standing.LiveLosses,
                            standing.LiveTies,
                            standing.LivePoints,
                            standing.Distance,
                            standing.DistancePerGame,
                            standing.PointsPerGame,
                            LastUpdated = DateTime.UtcNow // Update LastUpdated field
                        });
                    }
                    else
                    {
                        standing.TeamName = teamName;
                        standing.LastUpdated = DateTime.UtcNow;
                        await _context.LiigaStandings.AddAsync(standing);
                    }
                }               
            }
            await _context.SaveChangesAsync();
            return Ok("Standings data populated successfully");
        }

        private string ExtractAndFormatTeamName(string teamId)
        {
            if (string.IsNullOrEmpty(teamId) || !teamId.Contains(':'))
            {
                return string.Empty;
            }

            var parts = teamId.Split(':');
            if (parts.Length < 2)
            {
                return string.Empty;
            }

            var teamName = parts[1];
            return teamName.Length <= 4
                ? teamName.ToUpper()
                : char.ToUpper(teamName[0]) + teamName.Substring(1).ToLower();
        }

        private string FormatPercentage(string percentage)
        {
            if (string.IsNullOrEmpty(percentage))
            {
                return percentage;
            }

            if (double.TryParse(percentage, out double value))
            {
                return Math.Round(value, 2).ToString("F2");
            }

            return percentage;
        }
    }
}

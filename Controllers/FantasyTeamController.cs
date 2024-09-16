using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using SporttiporssiAPI.Models;
using SporttiporssiAPI.Models.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using SporttiporssiAPI.Models.DBModels;
using Microsoft.EntityFrameworkCore;

namespace SporttiporssiAPI.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    public class FantasyTeamController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;

        public FantasyTeamController(ApplicationDbContext context, HttpClient httpClient)
        {
            _context = context;
            _httpClient = httpClient;
        }

        [HttpGet]
        public async Task<IEnumerable<FantasyTeam>> GetAllTeamsBySerie(string serie)
        {
            var series = _context.Series.Where(s => s.SerieName == serie).FirstOrDefault();
            return _context.FantasyTeams.Where(t => t.Serie == series.SerieId);
        }

        [HttpGet("HockeyDefault")]
        public async Task<HockeyDefaultFTP> GetHockeyDefaultFTPBySerie(string serie)
        {
            var series = _context.Series.Where(s => s.SerieName == serie).FirstOrDefault();
            return _context.HockeyDefaultFTPs.Where(s => s.Serie == series.SerieId).FirstOrDefault();
        }

        [HttpGet("AllByUser")]
        public async Task<IEnumerable<FantasyTeam>> GetAllTeamsByUser(string serie, string email)
        {
            var user = _context.Users.Where(u => u.Email.ToLower() == email.ToLower()).FirstOrDefault();
            if (user == null)
            {
                Console.WriteLine("Could not find user.");
                return null;
            }
            var series = _context.Series.Where(s => s.SerieName == serie).FirstOrDefault();
            return _context.FantasyTeams.Where(t => t.Serie == series.SerieId && t.UserId == user.UserId);
        }

        [HttpGet("GetTeamByNameAndSerie")]
        public async Task<IEnumerable<FantasyTeam>> GetTeamByNameAndSerie(string serie, string teamName)
        {          
            var series = _context.Series.Where(s => s.SerieName == serie).FirstOrDefault();
            var team = _context.FantasyTeams.Where(t => t.Serie == series.SerieId && t.Teamname == teamName);
            return team;
        }

        [HttpGet("CanTrade")]
        public async Task<bool> CanTrade(string serie)
        {
            var serieId = await _context.Series.Where(s => s.SerieName.ToLower() == serie).Select(s => s.SerieId).FirstOrDefaultAsync();
            var league = new SqlParameter("@League", serieId);
            if (string.IsNullOrEmpty(serieId.ToString()))
            {
                return false;
            }
            var result = await _context.CanTradeResults.FromSqlRaw("EXEC CanTrade @League", league).ToListAsync();
            return result.FirstOrDefault()?.CanTrade ?? false;
        }
     
        [HttpPost]
        public async Task<ActionResult> SaveNewTeam([FromBody] TeamRegisterRequest request)
        {
            if (request == null || request.TeamName == null)
            {
                return BadRequest("Invalid request payload.");
            }
            var user = _context.Users.Where(u => u.Email.ToLower() == request.Email.ToLower()).FirstOrDefault();
            if (user == null)
            {
                return NotFound("User not found.");
            }
            var series = _context.Series.Where(s => s.SerieName == request.Serie).FirstOrDefault();
            bool nameInUse = _context.FantasyTeams.Where(t => t.Teamname == request.TeamName && t.Serie == series.SerieId).Any();
            if (nameInUse)
            {
                return Conflict("Team name is already in use.");
            }
            var newTeam = new FantasyTeam
            {
                UserId = user.UserId,
                Teamname = request.TeamName,
                Serie = series.SerieId,     
                TradesThisPhase = request.TradesThisPhase,
                FundsLeft = request.FundsLeft,
                TotalFTP = 0,
                LastUpdated = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,               
            };
            _context.FantasyTeams.Add(newTeam);
            await _context.SaveChangesAsync();
            return Ok(new { Success = true, Message = "Team created successfully" });
        }
    }
}

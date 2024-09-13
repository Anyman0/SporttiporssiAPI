using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using SporttiporssiAPI.Models;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace SporttiporssiAPI.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    public class GroupController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;

        public GroupController(ApplicationDbContext context, HttpClient httpClient)
        {
            _context = context;
            _httpClient = httpClient;
        }

        /// <summary>
        /// Gets all groups by serie
        /// </summary>
        /// <param name="serie"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IEnumerable<FantasyGroup>> GetAllGroupsBySerie(string serie)
        {                 
            var series = _context.Series.Where(s => s.SerieName == serie).FirstOrDefault();
            var list = _context.FantasyGroups.Where(g => g.Serie == series.SerieId).ToList();
            return _context.FantasyGroups.Where(g => g.Serie == series.SerieId);
        }

        /// <summary>
        /// Creates a new group
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> SaveNewGroup([FromBody] GroupRegisterRequest request)
        {
            if (request == null || request.FantasyGroup == null)
            {
                return BadRequest("Invalid request payload.");
            }
            var user = _context.Users.Where(u => u.Email.ToLower() == request.Email.ToLower()).FirstOrDefault();
            if (user == null)
            {
                return NotFound("User not found.");
            }
            var series = _context.Series.Where(s => s.SerieName == request.Serie).FirstOrDefault();
            bool nameInUse = _context.FantasyGroups.Where(f => f.GroupName == request.FantasyGroup.GroupName).Any();
            if (nameInUse)
            {
                return Conflict("Group name is already in use.");
            }
            // Salt and hashing for group password
            var salt = GenerateSalt();
            var passwordHash = HashPassword(request.Password, salt);
            var newGroup = new FantasyGroup
            {
                GroupName = request.FantasyGroup.GroupName,
                Serie = series.SerieId,
                OffencePassFTP = request.FantasyGroup.OffencePassFTP,
                OffenceGoalFTP = request.FantasyGroup.OffenceGoalFTP,
                OffencePenaltyFTP = request.FantasyGroup.OffencePenaltyFTP,
                OffencePenalty10FTP = request.FantasyGroup.OffencePenalty10FTP,
                OffencePenalty20FTP = request.FantasyGroup.OffencePenalty20FTP,
                OffenceShotFTP = request.FantasyGroup.OffenceShotFTP,
                OffencePowerFTP = request.FantasyGroup.OffencePowerFTP, // This is +/- statistics
                DefencePassFTP = request.FantasyGroup.DefencePassFTP,
                DefenceGoalFTP = request.FantasyGroup.DefenceGoalFTP,
                DefencePenaltyFTP = request.FantasyGroup.DefencePenaltyFTP,
                DefencePenalty10FTP = request.FantasyGroup.DefencePenalty10FTP,
                DefencePenalty20FTP = request.FantasyGroup.DefencePenalty20FTP,
                DefenceShotFTP = request.FantasyGroup.DefenceShotFTP,
                DefencePowerFTP = request.FantasyGroup.DefencePowerFTP, // This is +/- statistics
                GoaliePassFTP = request.FantasyGroup.GoaliePassFTP,
                GoalieGoalFTP = request.FantasyGroup.GoalieGoalFTP,
                GoalieSaveFTP = request.FantasyGroup.GoalieSaveFTP,
                GoalieWinFTP = request.FantasyGroup.GoalieWinFTP,
                GoalieShutoutFTP = request.FantasyGroup.GoalieShutoutFTP,
                FaceOffFTP = request.FantasyGroup.FaceOffFTP,
                StartMoney = request.FantasyGroup.StartMoney,
                CreatedBy = user.UserId,
                CreatedDate = DateTime.UtcNow,
                GroupPasswordHash = passwordHash,
                Salt = salt,
                TradesPerPhase = request.FantasyGroup.TradesPerPhase,
            };
            try
            {
                _context.FantasyGroups.Add(newGroup);
                await _context.SaveChangesAsync();
                return Ok(new { Success = true, Message = "Group created successfully" });
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"Database update error: {dbEx.Message}");
                // Optionally log inner exception
                if (dbEx.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {dbEx.InnerException.Message}");
                }
                // Handle or rethrow as needed
                throw;
            }     
            catch(Exception ex)
            {
                // Log general errors
                Console.WriteLine($"General error: {ex.Message}");
                // Handle or rethrow as needed
                throw;
            }           
        }

        [HttpGet("GetGroupDataAndStandingByTeamId")]
        public async Task<IActionResult> GetGroupDataAndStandingByTeamId(Guid teamId)
        {
            try
            {
                var results = await _context.GetGroupDataAndStandingByTeamId(teamId);
                if (results == null || !results.Any())
                {
                    return NotFound("Team not in any group");
                }
                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("JoinGroup")]
        public async Task<ActionResult> JoinGroup([FromBody] RegisterRequest request, Guid groupId, Guid teamId )
        {
            var group = _context.FantasyGroups.Where(g => g.GroupId == groupId).FirstOrDefault();
            if(group == null)
            {
                return NotFound("Group not found.");
            }
            var team = _context.FantasyTeams.Where(ft => ft.FantasyTeamId == teamId).FirstOrDefault();
            if (team == null)
            {
                return NotFound("Team not found.");
            }
            var hashedPassword = HashPassword(request.Password, group.Salt);
            if(hashedPassword != group.GroupPasswordHash)
            {
                return Unauthorized("Invalid password");
            }
            var newLink = new FantasyGroupTeamLink
            {
                GroupId = groupId,
                FantasyTeamId = teamId,
                TotalGroupFTP = 0,
            };
            _context.FantasyGroupTeamLinks.Add(newLink);
            await _context.SaveChangesAsync();
            return Ok(new { Success = true, Message = "Link added" });
        }

        [HttpDelete("LeaveGroup")]
        public async Task<ActionResult> LeaveGroup(string groupName, string serie, Guid teamId)
        {
            var series = _context.Series.Where(s => s.SerieName == serie).FirstOrDefault();
            if (series == null) return NotFound("Serie not found");
            var group = _context.FantasyGroups.Where(g => g.GroupName == groupName && g.Serie == series.SerieId).FirstOrDefault();
            if (group == null)
            {
                return NotFound("Group not found.");
            }
            var link = _context.FantasyGroupTeamLinks.Where(ft => ft.FantasyTeamId == teamId && ft.GroupId == group.GroupId).FirstOrDefault();
            if(link != null)
            {
                _context.FantasyGroupTeamLinks.Remove(link);
                await _context.SaveChangesAsync();
                return Ok(new { Success = true, Message = "Link removed" });
            }
            return BadRequest();
        }

        private string GenerateSalt(int size = 16)
        {
            var buffer = new byte[size];
            RandomNumberGenerator.Create().GetBytes(buffer);
            return Convert.ToBase64String(buffer);
        }

        private string HashPassword(string password, string salt)
        {
            var sha256 = SHA256.Create();
            var saltedPassword = password + salt;
            byte[] saltedPasswordBytes = System.Text.Encoding.UTF8.GetBytes(saltedPassword);
            byte[] hashBytes = sha256.ComputeHash(saltedPasswordBytes);
            return Convert.ToBase64String(hashBytes);
        }
    }
}

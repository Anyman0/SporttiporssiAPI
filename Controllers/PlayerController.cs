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
    public class PlayerController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly string liigaBaseAddress = "https://www.liiga.fi/api/v2/";

        public PlayerController(ApplicationDbContext context, HttpClient httpClient)
        {
            _context = context;
            _httpClient = httpClient;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Player>>> GetPlayers()
        {
            return await _context.Players.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Player>> GetPlayer(Guid id)
        {
            var player = await _context.Players.FindAsync(id);

            if (player == null)
            {
                return NotFound();
            }

            return player;
        }

        [HttpGet("PlayersByFantasyTeam")]
        public async Task<ActionResult<IEnumerable<Player>>> GetPlayersByFantasyTeamName(string teamName)
        {
            var fantasyTeamId = _context.FantasyTeams.Where(f => f.Teamname == teamName).FirstOrDefault().FantasyTeamId;
            var teamPlayerIds = await _context.FantasyTeamPlayerLinks.Where(ft => ft.FantasyTeamId == fantasyTeamId).Select(ft => ft.PlayerId).ToListAsync();
            var players = await _context.Players.Where(p => teamPlayerIds.Contains(p.PlayerId)).ToListAsync();
            return Ok(players);
        }

        [HttpPost("PostPlayer")]
        public async Task<ActionResult<Player>> PostPlayer(Player player)
        {
            _context.Players.Add(player);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPlayer), new { id = player.PlayerId }, player);
        }

        [HttpPost("AddPlayerToTeam")]
        public async Task<ActionResult<Player>> AddToTeam(int playerId, Guid fantasyTeamId)
        {
            var playerToAdd = _context.Players.Where(p => p.PlayerId == playerId).FirstOrDefault();
            var fantasyTeam = _context.FantasyTeams.Where(t => t.FantasyTeamId == fantasyTeamId).FirstOrDefault();
            var sportPhase = _context.SportPhases.Where(s => s.SerieId == fantasyTeam.Serie).
                Where(p => p.Active == true && p.StartDate <= DateTime.Now).FirstOrDefault();           
            if (fantasyTeam != null && sportPhase != null) // Only adding trades IF phase has started
            {
                fantasyTeam.TradesThisPhase++;
            }
            var existingLink = await _context.FantasyTeamPlayerLinks.Join(_context.Players, link => link.PlayerId, player => player.PlayerId,
                (link, player) => new { link, player }).FirstOrDefaultAsync(joined => joined.link.FantasyTeamId == fantasyTeamId
                && joined.player.Role == playerToAdd.Role);
            if (existingLink != null)
            {
                existingLink.link.PlayerId = playerToAdd.PlayerId;
                _context.FantasyTeamPlayerLinks.Update(existingLink.link);
                await _context.SaveChangesAsync();
                return Ok(existingLink.link);
            }
            else
            {
                var newPlayerLink = new FantasyTeamPlayerLink
                {
                    FTPLinkId = Guid.NewGuid(),
                    PlayerId = playerToAdd.PlayerId,
                    FantasyTeamId = fantasyTeamId
                };
                _context.FantasyTeamPlayerLinks.Add(newPlayerLink);
                await _context.SaveChangesAsync();
                return Ok(newPlayerLink);
            }            
        }


        [HttpPost("PopulatePlayersToDB")]
        public async Task<ActionResult>PopulatePlayersToDB()
        {
            var apiUrl = liigaBaseAddress + "players/stats/summed/2025/2025/runkosarja/true?dataType=basicStats";
            var response = await _httpClient.GetAsync(apiUrl);

            if(!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, "Error fetching data from liiga-API");
            }

            var json = await response.Content.ReadAsStringAsync();
            var players = JsonConvert.DeserializeObject<List<Player>>(json);

            if(players == null || !players.Any())
            {
                return BadRequest("No players data available");
            }

            foreach(var player in players)
            {
                var existingPlayer = await _context.Players.FirstOrDefaultAsync(p => p.PlayerId == player.PlayerId);
                if(existingPlayer != null)
                {
                    existingPlayer.TeamId = player.TeamId;
                    existingPlayer.TeamName = player.TeamName;
                    existingPlayer.TeamShortName = player.TeamShortName;
                    existingPlayer.Role = player.Role;
                    existingPlayer.FirstName = player.FirstName;
                    existingPlayer.LastName = player.LastName;
                    existingPlayer.Nationality = player.Nationality;
                    existingPlayer.Tournament = player.Tournament;
                    existingPlayer.PictureUrl = player.PictureUrl;
                    existingPlayer.PreviousTeamsForTournament = player.PreviousTeamsForTournament;
                    existingPlayer.Injured = player.Injured;
                    existingPlayer.Jersey = player.Jersey;
                    existingPlayer.LastSeason = player.LastSeason;
                    existingPlayer.Goalkeeper = player.Goalkeeper;
                    existingPlayer.Games = player.Games;
                    existingPlayer.PlayedGames = player.PlayedGames;
                    existingPlayer.Rookie = player.Rookie;
                    existingPlayer.Suspended = player.Suspended;
                    existingPlayer.Removed = player.Removed;
                    existingPlayer.TimeOnIce = player.TimeOnIce;
                    existingPlayer.Current = player.Current;
                    existingPlayer.Goals = player.Goals;
                    existingPlayer.Assists = player.Assists;
                    existingPlayer.Points = player.Points;
                    existingPlayer.Plus = player.Plus;
                    existingPlayer.Minus = player.Minus;
                    existingPlayer.PlusMinus = player.PlusMinus;
                    existingPlayer.PenaltyMinutes = player.PenaltyMinutes;
                    existingPlayer.PowerplayGoals = player.PowerplayGoals;
                    existingPlayer.PenaltykillGoals = player.PenaltykillGoals;
                    existingPlayer.WinningGoals = player.WinningGoals;
                    existingPlayer.Shots = player.Shots;
                    existingPlayer.ShotsIntoGoal = player.ShotsIntoGoal;
                    existingPlayer.FaceoffsWon = player.FaceoffsWon;
                    existingPlayer.FaceoffsLost = player.FaceoffsLost;
                    existingPlayer.ExpectedGoals = player.ExpectedGoals;
                    existingPlayer.TimeOnIceAvg = player.TimeOnIceAvg;
                    existingPlayer.FaceoffWonPercentage = player.FaceoffWonPercentage;
                    existingPlayer.ShotPercentage = player.ShotPercentage;
                    existingPlayer.FaceoffsTotal = player.FaceoffsTotal;
                    existingPlayer.LastUpdated = DateTime.UtcNow;
                    existingPlayer.FTP = player.FTP;
                    existingPlayer.PlayerOwned = player.PlayerOwned;

                    _context.Players.Update(existingPlayer);
                }
                else
                {
                    player.LastUpdated = DateTime.UtcNow;
                    await _context.Players.AddAsync(player);
                }
            }
            await _context.SaveChangesAsync();
            return Ok("Players data populated successfully");
        }

        [HttpPut]
        public async Task<IActionResult> UpdatePlayerPrice(int playerId, int price)
        {
            if (playerId == null)
            {
                return BadRequest();
            }
            var playerToUpdate = await _context.Players.Where(p => p.PlayerId == playerId).FirstOrDefaultAsync();
            playerToUpdate.LastUpdated = DateTime.UtcNow;
            playerToUpdate.Price = price;
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("UpdatePlayerPenalties")]
        public async Task<IActionResult> UpdatePlayerPenalties(int playerId, int two, int ten, int twenty)
        {
            if (playerId < 10)
            {
                return BadRequest();
            }
            var fantasyGroups = await _context.FantasyGroups.ToListAsync();
            var fantasyTeamPlayerLinks = await _context.FantasyTeamPlayerLinks.ToListAsync();
            var fantasyGroupTeamLinks = await _context.FantasyGroupTeamLinks.ToListAsync();
            var hockeyDefaultFTP = await _context.HockeyDefaultFTPs.FirstOrDefaultAsync();

            // Update to player
            var playerToUpdate = await _context.Players.Where(p => p.PlayerId == playerId).FirstOrDefaultAsync();
            if (playerToUpdate != null)
            {
                playerToUpdate.LastUpdated = DateTime.UtcNow;
                playerToUpdate.Penalty2 += two;
                playerToUpdate.Penalty10 += ten;
                playerToUpdate.Penalty20 += twenty;

                if (playerToUpdate.Role == "OL" || playerToUpdate.Role == "KH" || playerToUpdate.Role == "VL")
                {
                    playerToUpdate.FTP += hockeyDefaultFTP.OffencePenaltyFTP * two;
                    playerToUpdate.FTP += hockeyDefaultFTP.OffencePenalty10FTP * ten;
                    playerToUpdate.FTP += hockeyDefaultFTP.OffencePenalty20FTP * twenty;
                }
                else if (playerToUpdate.Role == "VP" || playerToUpdate.Role == "OP")
                {
                    playerToUpdate.FTP += hockeyDefaultFTP.DefencePenaltyFTP * two;
                    playerToUpdate.FTP += hockeyDefaultFTP.DefencePenalty10FTP * ten;
                    playerToUpdate.FTP += hockeyDefaultFTP.DefencePenalty20FTP * twenty;
                }
                else if (playerToUpdate.Role == "MV")
                {
                    // GoaliePenaltyFTPs
                }
            }

            // Update to fantasyteam
            var playerTeams = await _context.FantasyTeamPlayerLinks.Where(p => p.PlayerId == playerId).ToListAsync();            
            foreach(var team in playerTeams)
            {
                var teamGroupLink = await _context.FantasyGroupTeamLinks.Where(g => g.FantasyTeamId == team.FantasyTeamId).FirstOrDefaultAsync();
                var groupTeam = await _context.FantasyTeamStats.Where(t => t.FantasyTeamId == team.FantasyTeamId).FirstOrDefaultAsync();
                // Team is in a group. Getting that groups set FTP count values
                if(teamGroupLink != null)
                {
                    var teamGroup = await _context.FantasyGroups.Where(g => g.GroupId == teamGroupLink.GroupId).FirstOrDefaultAsync();
                    if (teamGroup != null)
                    {
                        if (playerToUpdate.Role == "OL" || playerToUpdate.Role == "KH" || playerToUpdate.Role == "VL")
                        {
                            groupTeam.TotalFTP += teamGroup.OffencePenaltyFTP * two;
                            groupTeam.TotalFTP += teamGroup.OffencePenalty10FTP * ten;
                            groupTeam.TotalFTP += teamGroup.OffencePenalty20FTP * twenty;
                        }
                        else if (playerToUpdate.Role == "VP" || playerToUpdate.Role == "OP")
                        {
                            groupTeam.TotalFTP += teamGroup.DefencePenaltyFTP * two;
                            groupTeam.TotalFTP += teamGroup.DefencePenalty10FTP * ten;
                            groupTeam.TotalFTP += teamGroup.DefencePenalty20FTP * twenty;
                        }
                        else if (playerToUpdate.Role == "MV")
                        {
                            // GoaliePenaltyFTPs
                        }
                    }
                }
                // Team is not in a group, updating with hockeydefaultFTP
                else
                {
                    if (hockeyDefaultFTP != null)
                    {
                        if (playerToUpdate.Role == "OL" || playerToUpdate.Role == "KH" || playerToUpdate.Role == "VL")
                        {
                            groupTeam.TotalFTP += hockeyDefaultFTP.OffencePenaltyFTP * two;
                            groupTeam.TotalFTP += hockeyDefaultFTP.OffencePenalty10FTP * ten;
                            groupTeam.TotalFTP += hockeyDefaultFTP.OffencePenalty20FTP * twenty;
                        }
                        else if (playerToUpdate.Role == "VP" || playerToUpdate.Role == "OP")
                        {
                            groupTeam.TotalFTP += hockeyDefaultFTP.DefencePenaltyFTP * two;
                            groupTeam.TotalFTP += hockeyDefaultFTP.DefencePenalty10FTP * ten;
                            groupTeam.TotalFTP += hockeyDefaultFTP.DefencePenalty20FTP * twenty;
                        }
                        else if (playerToUpdate.Role == "MV")
                        {
                            // GoaliePenaltyFTPs
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("UpdatePlayerStats")]
        public async Task<IActionResult> UpdatePlayerStats(UpdatePlayer player)
        {
            if (player.PlayerId < 10)
            {
                return BadRequest();
            }
            var fantasyGroups = await _context.FantasyGroups.ToListAsync();
            var fantasyTeamPlayerLinks = await _context.FantasyTeamPlayerLinks.ToListAsync();
            var fantasyGroupTeamLinks = await _context.FantasyGroupTeamLinks.ToListAsync();
            var hockeyDefaultFTP = await _context.HockeyDefaultFTPs.FirstOrDefaultAsync();

            // Update to player
            var playerToUpdate = await _context.Players.Where(p => p.PlayerId == player.PlayerId).FirstOrDefaultAsync();
            if (playerToUpdate != null)
            {
                playerToUpdate.LastUpdated = DateTime.UtcNow;
                playerToUpdate.PlayedGames++;
                playerToUpdate.Penalty2 += player.Penalty2;
                playerToUpdate.Penalty10 += player.Penalty10;
                playerToUpdate.Penalty20 += player.Penalty20;
                playerToUpdate.Points += player.Points;
                playerToUpdate.Goals += player.Goals;
                playerToUpdate.Assists += player.Assists;
                playerToUpdate.Plus += player.Plus;
                playerToUpdate.Minus += player.Minus;
                playerToUpdate.PlusMinus += player.PlusMinus;
                playerToUpdate.PenaltyMinutes += player.PenaltyMinutes;
                playerToUpdate.Shots += player.Shots;
                playerToUpdate.FaceoffsTotal += player.FaceoffsTotal; 
                playerToUpdate.Saves += player.Saves;
                playerToUpdate.GameWon += player.GameWon;
                playerToUpdate.GoalieShutout += player.GoalieShutout;
                playerToUpdate.AllowedGoals += player.AllowedGoals;
                decimal faceoffDecimal = (decimal)playerToUpdate.FaceoffWonPercentage / 100;
                int faceoffsWon = (int)Math.Round(faceoffDecimal * (int)player.FaceoffsTotal);
                playerToUpdate.FaceoffsWon += faceoffsWon;
                playerToUpdate.FaceoffsLost += (int)player.FaceoffsTotal - faceoffsWon;
                playerToUpdate.TimeOnIce += player.TimeOnIce;               

                // Calculate averages
                playerToUpdate.TimeOnIceAvg = TimeSpan.FromSeconds((int)playerToUpdate.TimeOnIce / playerToUpdate.PlayedGames).ToString(@"mm\:ss");
                playerToUpdate.FaceoffWonPercentage = playerToUpdate.FaceoffsTotal > 0 ? ((float)playerToUpdate.FaceoffsWon / playerToUpdate.FaceoffsTotal) * 100 : 0;
                playerToUpdate.ShotPercentage = playerToUpdate.Shots > 0 ? ((float)playerToUpdate.Goals / playerToUpdate.Shots) * 100 : 0;

                if (playerToUpdate.Role == "OL" || playerToUpdate.Role == "KH" || playerToUpdate.Role == "VL")
                {
                    playerToUpdate.FTP += hockeyDefaultFTP.OffencePenaltyFTP * player.Penalty2;
                    playerToUpdate.FTP += hockeyDefaultFTP.OffencePenalty10FTP * player.Penalty10;
                    playerToUpdate.FTP += hockeyDefaultFTP.OffencePenalty20FTP * player.Penalty20;
                    playerToUpdate.FTP += hockeyDefaultFTP.OffencePassFTP * player.Assists;
                    playerToUpdate.FTP += hockeyDefaultFTP.OffenceGoalFTP * player.Goals;                    
                    playerToUpdate.FTP += (int)hockeyDefaultFTP.OffenceShotFTP * player.Shots;
                    playerToUpdate.FTP += (int)hockeyDefaultFTP.OffencePowerFTP * player.PlusMinus;
                }
                else if (playerToUpdate.Role == "VP" || playerToUpdate.Role == "OP")
                {
                    playerToUpdate.FTP += hockeyDefaultFTP.DefencePenaltyFTP * player.Penalty2;
                    playerToUpdate.FTP += hockeyDefaultFTP.DefencePenalty10FTP * player.Penalty10;
                    playerToUpdate.FTP += hockeyDefaultFTP.DefencePenalty20FTP * player.Penalty20;
                    playerToUpdate.FTP += hockeyDefaultFTP.DefencePassFTP * player.Assists;
                    playerToUpdate.FTP += hockeyDefaultFTP.DefenceGoalFTP * player.Goals;
                    playerToUpdate.FTP += (int)(hockeyDefaultFTP.DefenceShotFTP * player.Shots);
                    playerToUpdate.FTP += (int)(hockeyDefaultFTP.DefencePowerFTP * player.PlusMinus);
                }
                else if (playerToUpdate.Role == "MV")
                {                   
                    playerToUpdate.FTP += hockeyDefaultFTP.GoaliePassFTP * player.Assists;
                    playerToUpdate.FTP += hockeyDefaultFTP.GoalieGoalFTP * player.Goals;
                    playerToUpdate.FTP += hockeyDefaultFTP.GoalieWinFTP * player.GameWon;
                    playerToUpdate.FTP += (int)(hockeyDefaultFTP.GoalieSaveFTP * player.Saves);
                    playerToUpdate.FTP += hockeyDefaultFTP.GoalieShutoutFTP * player.GoalieShutout;
                }
            }

            // Update to fantasyteam
            var playerTeams = await _context.FantasyTeamPlayerLinks.Where(p => p.PlayerId == player.PlayerId).ToListAsync();
            foreach (var team in playerTeams)
            {
                var teamGroupLink = await _context.FantasyGroupTeamLinks.Where(g => g.FantasyTeamId == team.FantasyTeamId).FirstOrDefaultAsync();
                var groupTeam = await _context.FantasyTeamStats.Where(t => t.FantasyTeamId == team.FantasyTeamId).FirstOrDefaultAsync();
                if (groupTeam == null)
                {
                    // Create a new entry if not found
                    groupTeam = new FantasyTeamStats
                    {
                        FantasyTeamId = team.FantasyTeamId,
                        TotalFTP = 0,
                        Goals = 0,
                        Assists = 0,
                        Shots = 0,
                        PenaltyMinutes = 0,
                        FaceoffWins = 0,
                        PlusMinus = 0,
                        Saves = 0,
                        BlockedShots = 0,
                        Distance = 0
                    };

                    _context.FantasyTeamStats.Add(groupTeam);
                }
                // Team is in a group. Getting that groups set FTP count values
                if (teamGroupLink != null)
                {
                    var teamGroup = await _context.FantasyGroups.Where(g => g.GroupId == teamGroupLink.GroupId).FirstOrDefaultAsync();
                    if (teamGroup != null)
                    {
                        if (playerToUpdate.Role == "OL" || playerToUpdate.Role == "KH" || playerToUpdate.Role == "VL")
                        {
                            groupTeam.TotalFTP += teamGroup.OffencePenaltyFTP * (int)player.Penalty2;
                            groupTeam.TotalFTP += teamGroup.OffencePenalty10FTP * (int)player.Penalty10;
                            groupTeam.TotalFTP += teamGroup.OffencePenalty20FTP * (int)player.Penalty20;
                            groupTeam.TotalFTP += teamGroup.OffencePassFTP * (int)player.Assists;
                            groupTeam.TotalFTP += teamGroup.OffenceGoalFTP * (int)player.Goals;
                            groupTeam.TotalFTP += (int)(teamGroup.OffenceShotFTP * (int)player.Shots);
                            groupTeam.TotalFTP += (int)(teamGroup.OffencePowerFTP * (int)player.PlusMinus);
                        }
                        else if (playerToUpdate.Role == "VP" || playerToUpdate.Role == "OP")
                        {
                            groupTeam.TotalFTP += teamGroup.DefencePenaltyFTP * (int)player.Penalty2;
                            groupTeam.TotalFTP += teamGroup.DefencePenalty10FTP * (int)player.Penalty10;
                            groupTeam.TotalFTP += teamGroup.DefencePenalty20FTP * (int)player.Penalty20;
                            groupTeam.TotalFTP += teamGroup.DefencePassFTP * (int)player.Assists;
                            groupTeam.TotalFTP += teamGroup.DefenceGoalFTP * (int)player.Goals;
                            groupTeam.TotalFTP += (int)(teamGroup.DefenceShotFTP * (int)player.Shots);
                            groupTeam.TotalFTP += (int)(teamGroup.DefencePowerFTP * (int)player.PlusMinus);
                        }
                        else if (playerToUpdate.Role == "MV")
                        {
                            groupTeam.TotalFTP += teamGroup.GoaliePassFTP * (int)player.Assists;
                            groupTeam.TotalFTP += teamGroup.GoalieGoalFTP * (int)player.Goals;
                            groupTeam.TotalFTP += teamGroup.GoalieWinFTP * (int)player.GameWon;
                            groupTeam.TotalFTP += (int)(teamGroup.GoalieSaveFTP * player.Saves);
                            groupTeam.TotalFTP += teamGroup.GoalieShutoutFTP * (int)player.GoalieShutout;
                        }
                    }
                }
                // Team is not in a group, updating with hockeydefaultFTP
                else
                {
                    if (hockeyDefaultFTP != null)
                    {
                        if (playerToUpdate.Role == "OL" || playerToUpdate.Role == "KH" || playerToUpdate.Role == "VL")
                        {
                            groupTeam.TotalFTP += hockeyDefaultFTP.OffencePenaltyFTP * (int)player.Penalty2;
                            groupTeam.TotalFTP += hockeyDefaultFTP.OffencePenalty10FTP * (int)player.Penalty10;
                            groupTeam.TotalFTP += hockeyDefaultFTP.OffencePenalty20FTP * (int)player.Penalty20;
                            groupTeam.TotalFTP += hockeyDefaultFTP.OffencePassFTP * (int)player.Assists;
                            groupTeam.TotalFTP += hockeyDefaultFTP.OffenceGoalFTP * (int)player.Goals;
                            groupTeam.TotalFTP += (int)(hockeyDefaultFTP.OffenceShotFTP * (int)player.Shots);
                            groupTeam.TotalFTP += (int)(hockeyDefaultFTP.OffencePowerFTP * (int)player.PlusMinus);
                        }
                        else if (playerToUpdate.Role == "VP" || playerToUpdate.Role == "OP")
                        {
                            groupTeam.TotalFTP += hockeyDefaultFTP.DefencePenaltyFTP * (int)player.Penalty2;
                            groupTeam.TotalFTP += hockeyDefaultFTP.DefencePenalty10FTP * (int)player.Penalty10;
                            groupTeam.TotalFTP += hockeyDefaultFTP.DefencePenalty20FTP * (int)player.Penalty20;
                            groupTeam.TotalFTP += hockeyDefaultFTP.DefencePassFTP * (int)player.Assists;
                            groupTeam.TotalFTP += hockeyDefaultFTP.DefenceGoalFTP * (int)player.Goals;
                            groupTeam.TotalFTP += (int)(hockeyDefaultFTP.DefenceShotFTP * (int)player.Shots);
                            groupTeam.TotalFTP += (int)(hockeyDefaultFTP.DefencePowerFTP * (int)player.PlusMinus);
                        }
                        else if (playerToUpdate.Role == "MV")
                        {
                            groupTeam.TotalFTP += hockeyDefaultFTP.GoaliePassFTP * (int)player.Assists;
                            groupTeam.TotalFTP += hockeyDefaultFTP.GoalieGoalFTP * (int)player.Goals;
                            groupTeam.TotalFTP += hockeyDefaultFTP.GoalieWinFTP * (int)player.GameWon;
                            groupTeam.TotalFTP += (int)(hockeyDefaultFTP.GoalieSaveFTP * player.Saves);
                            groupTeam.TotalFTP += (int)(hockeyDefaultFTP.GoalieShutoutFTP * player.GoalieShutout);
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("UpdateGoalieStats")]
        public async Task<IActionResult> UpdateGoalieStats(int playerId, int saves, int won, int goals, int assists, 
            int goalsAllowed, int penalty2, int penalty10, int penalty20)
        {
            if (playerId == null)
            {
                return BadRequest();
            }
            var playerToUpdate = await _context.Players.Where(p => p.PlayerId == playerId).FirstOrDefaultAsync();
            
            playerToUpdate.LastUpdated = DateTime.UtcNow;
            playerToUpdate.Saves += saves;
            playerToUpdate.GameWon += won;
            playerToUpdate.Goals += goals;
            playerToUpdate.Assists += assists;
            //playerToUpdate.AllowedGoals += goalsAllowed;
            playerToUpdate.Penalty2 += penalty2;
            playerToUpdate.Penalty10 += penalty10;
            playerToUpdate.Penalty20 += penalty20;
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id}")]
        private async Task<IActionResult> DeletePlayer(Guid id)
        {
            var player = await _context.Players.FindAsync(id);

            if (player == null)
            {
                return NotFound();
            }

            _context.Players.Remove(player);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

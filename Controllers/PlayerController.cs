using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace SporttiporssiAPI.Controllers
{
    [Authorize]
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

        [HttpPost("PostPlayer")]
        public async Task<ActionResult<Player>> PostPlayer(Player player)
        {
            _context.Players.Add(player);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPlayer), new { id = player.PlayerId }, player);
        }

        [HttpPost("PopulatePlayersToDB")]
        private async Task<ActionResult>PopulatePlayersToDB()
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
                    player.LastUpdated = DateTime.UtcNow;
                    _context.Entry(existingPlayer).CurrentValues.SetValues(player);
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

        [HttpPut("{id}")]
        private async Task<IActionResult> PutPlayer(int id, Player player)
        {
            if (id != player.PlayerId)
            {
                return BadRequest();
            }

            _context.Entry(player).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
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

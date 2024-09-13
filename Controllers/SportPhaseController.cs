using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SporttiporssiAPI.Models;
using System.Numerics;

namespace SporttiporssiAPI.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    public class SportPhaseController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;

        public SportPhaseController(ApplicationDbContext context, HttpClient client)
        {
            _context = context;
            _httpClient = client;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SportPhase>>> GetPhasesBySeries(string serie)
        {
            return await _context.SportPhases.ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult> AddSportPhase(SportPhase phase)
        {
            phase.SportPhaseId = Guid.NewGuid();
            _context.SportPhases.Add(phase);
            await _context.SaveChangesAsync();
            return Ok($"Added phase {phase.PhaseNumber} to serie {phase.SerieId}");
        }
    }
}

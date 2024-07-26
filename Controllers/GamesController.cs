using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SporttiporssiAPI.Models;
using System.Collections.ObjectModel;

namespace SporttiporssiAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class GamesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly string liigaBaseAddress = "https://www.liiga.fi/api/v2/";

        public GamesController(ApplicationDbContext context, HttpClient httpClient)
        {
            _context = context;
            _httpClient = httpClient;
        }

        [HttpGet]
        public async Task<ObservableCollection<Game>> GetGamesByDate(DateTime date)
        {
            var nextDay = date.AddDays(1);
            var gamesList = await _context.Games.Where(g => g.Start >= date && g.Start < nextDay).ToListAsync();
            var gamesCollection = new ObservableCollection<Game>(gamesList);
            return gamesCollection;
        }

        [HttpGet("LastGames")]
        public async Task<string> GetLastThreeGames(string teamName)
        {
            var gameResults = await _context.Games.Where(g => (g.HomeTeam.TeamName == teamName || g.AwayTeam.TeamName == teamName) && g.Ended)
                .OrderByDescending(g => g.Start).Take(3).Select(g => DetermineGameResult(g, teamName)).ToListAsync();
            gameResults.Reverse();
            return string.Join(" ", gameResults);
        }

        [HttpGet("LastGamesForAll")]
        public async Task<ActionResult<Dictionary<string, List<string>>>> GetLastGamesForAll()
        {
            var games = await _context.Games
                .Where(g => g.Ended)
                .OrderByDescending(g => g.Start).ToListAsync();
            var groupedGames = games.GroupBy(g => g.HomeTeam.TeamName).Union(games.GroupBy(g => g.AwayTeam.TeamName))
                .GroupBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.SelectMany(gg => gg).OrderByDescending(gg => gg.Start).Take(3).Reverse()
                .Select(gg => DetermineGameResult(gg, g.Key)).ToList());
            return Ok(groupedGames);
        }

        [HttpGet("{gameId}/{season}", Name = "GetGameStats")]
        public async Task<ActionResult<IEnumerable<GameStats>>> GetGameStats(int gameId, int season)
        {
            var apiUrl = liigaBaseAddress + $"games/{season}/{gameId}";
            var response = await _httpClient.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, "Error fetching data from liiga-API");
            }
            var json = await response.Content.ReadAsStringAsync();
            var gameStats = JsonConvert.DeserializeObject<GameStats>(json);
            if (gameStats == null)
            {
                return NotFound("No gamestats available");
            }
            return Ok(gameStats);
        }

        [AllowAnonymous]
        [HttpPost("{year}", Name = "PopulateLiigaGames")]
        public async Task<ActionResult> PopulateLiigaGames(int year)
        {
            var apiUrl = liigaBaseAddress + $"games?tournament=runkosarja&season={year}";
            var response = await _httpClient.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, "Error fetching data from liiga-API");
            }

            var json = await response.Content.ReadAsStringAsync();
            var games = JsonConvert.DeserializeObject<ObservableCollection<Game>>(json);

            if (games == null || !games.Any())
            {
                return BadRequest("No players data available");
            }

            foreach (var gameDto in games)
            {
                var existingGame = await _context.Games
                .Include(g => g.HomeTeam)
                .Include(g => g.AwayTeam)
                .FirstOrDefaultAsync(g => g.Id == gameDto.Id);

                if (existingGame != null)
                {
                    // Update existing game
                    existingGame.Season = gameDto.Season;
                    existingGame.Start = gameDto.Start;
                    existingGame.FinishedType = gameDto.FinishedType;
                    existingGame.Started = gameDto.Started;
                    existingGame.Ended = gameDto.Ended;
                    existingGame.BuyTicketsUrl = gameDto.BuyTicketsUrl;
                    existingGame.Stale = gameDto.Stale;
                    existingGame.Serie = gameDto.Serie;
                    existingGame.LastUpdated = DateTime.UtcNow;

                    // Update HomeTeam
                    existingGame.HomeTeam.TeamId = gameDto.HomeTeam.TeamId;
                    existingGame.HomeTeam.TeamName = gameDto.HomeTeam.TeamName;
                    existingGame.HomeTeam.Goals = gameDto.HomeTeam.Goals;
                    existingGame.HomeTeam.TimeOut = gameDto.HomeTeam.TimeOut;
                    existingGame.HomeTeam.PowerplayInstances = gameDto.HomeTeam.PowerplayInstances;
                    existingGame.HomeTeam.PowerplayGoals = gameDto.HomeTeam.PowerplayGoals;
                    existingGame.HomeTeam.ShortHandedInstances = gameDto.HomeTeam.ShortHandedInstances;
                    existingGame.HomeTeam.ShortHandedGoals = gameDto.HomeTeam.ShortHandedGoals;
                    existingGame.HomeTeam.Ranking = gameDto.HomeTeam.Ranking;
                    existingGame.HomeTeam.GameStartDateTime = gameDto.HomeTeam.GameStartDateTime;

                    // Update AwayTeam
                    existingGame.AwayTeam.TeamId = gameDto.AwayTeam.TeamId;
                    existingGame.AwayTeam.TeamName = gameDto.AwayTeam.TeamName;
                    existingGame.AwayTeam.Goals = gameDto.AwayTeam.Goals;
                    existingGame.AwayTeam.TimeOut = gameDto.AwayTeam.TimeOut;
                    existingGame.AwayTeam.PowerplayInstances = gameDto.AwayTeam.PowerplayInstances;
                    existingGame.AwayTeam.PowerplayGoals = gameDto.AwayTeam.PowerplayGoals;
                    existingGame.AwayTeam.ShortHandedInstances = gameDto.AwayTeam.ShortHandedInstances;
                    existingGame.AwayTeam.ShortHandedGoals = gameDto.AwayTeam.ShortHandedGoals;
                    existingGame.AwayTeam.Ranking = gameDto.AwayTeam.Ranking;
                    existingGame.AwayTeam.GameStartDateTime = gameDto.AwayTeam.GameStartDateTime;
                    _context.Entry(existingGame).CurrentValues.SetValues(existingGame);
                }
                else
                {
                    // Add new game
                    var newGame = new Game
                    {
                        Id = gameDto.Id,
                        Season = gameDto.Season,
                        Start = gameDto.Start,
                        HomeTeam = new Team
                        {
                            TeamId = gameDto.HomeTeam.TeamId,
                            TeamName = gameDto.HomeTeam.TeamName,
                            Goals = gameDto.HomeTeam.Goals,
                            TimeOut = gameDto.HomeTeam.TimeOut,
                            PowerplayInstances = gameDto.HomeTeam.PowerplayInstances,
                            PowerplayGoals = gameDto.HomeTeam.PowerplayGoals,
                            ShortHandedInstances = gameDto.HomeTeam.ShortHandedInstances,
                            ShortHandedGoals = gameDto.HomeTeam.ShortHandedGoals,
                            Ranking = gameDto.HomeTeam.Ranking,
                            GameStartDateTime = gameDto.HomeTeam.GameStartDateTime
                        },
                        AwayTeam = new Team
                        {
                            TeamId = gameDto.AwayTeam.TeamId,
                            TeamName = gameDto.AwayTeam.TeamName,
                            Goals = gameDto.AwayTeam.Goals,
                            TimeOut = gameDto.AwayTeam.TimeOut,
                            PowerplayInstances = gameDto.AwayTeam.PowerplayInstances,
                            PowerplayGoals = gameDto.AwayTeam.PowerplayGoals,
                            ShortHandedInstances = gameDto.AwayTeam.ShortHandedInstances,
                            ShortHandedGoals = gameDto.AwayTeam.ShortHandedGoals,
                            Ranking = gameDto.AwayTeam.Ranking,
                            GameStartDateTime = gameDto.AwayTeam.GameStartDateTime
                        },
                        FinishedType = gameDto.FinishedType,
                        Started = gameDto.Started,
                        Ended = gameDto.Ended,
                        BuyTicketsUrl = gameDto.BuyTicketsUrl,
                        Stale = gameDto.Stale,
                        Serie = gameDto.Serie,
                        LastUpdated = DateTime.UtcNow                       
                    };
                    await _context.Games.AddAsync(newGame);                   
                }               
            }
            await _context.SaveChangesAsync();
            return Ok("Games data populated successfully");
        }

        private static string DetermineGameResult(Game game, string teamName)
        {
            bool isHomeTeam = game.HomeTeam.TeamName == teamName;
            bool won = isHomeTeam ? game.HomeTeam.Goals > game.AwayTeam.Goals
                : game.AwayTeam.Goals > game.HomeTeam.Goals;
            return won ? "W" : "L";
        }
    }
}

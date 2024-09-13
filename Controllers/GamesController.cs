using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SporttiporssiAPI.Models;
using System.Collections.ObjectModel;
using SporttiporssiAPI.Helpers;
using SporttiporssiAPI.Models.DBModels;
using SporttiporssiAPI.Services;
using SporttiporssiAPI.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;

namespace SporttiporssiAPI.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    public class GamesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly IGameStatsService _gameStatsService;
        private readonly string _rapidKey;
        private readonly string _rapidHost;
        private readonly string liigaBaseAddress = "https://www.liiga.fi/api/v2/";
        private readonly string liveScoreBaseAddress = "https://livescore6.p.rapidapi.com/";

        public GamesController(ApplicationDbContext context, HttpClient httpClient, IConfiguration configuration, IGameStatsService gameStatsService)
        {
            _context = context;
            _httpClient = httpClient;
            _gameStatsService = gameStatsService;
            _rapidKey = configuration["RapidAPI:Key"];
            _rapidHost = configuration["RapidAPI:Host"];
        }

        [HttpGet]
        public async Task<ObservableCollection<Game>> GetGamesByDate(DateTime date, string serie)
        {
            var league = await _context.Series.Where(s => s.SerieName.ToLower() == serie.ToLower()).Select(x => x.SerieId).FirstOrDefaultAsync();
            var nextDay = date.AddDays(1);
            var gamesList = await _context.Games.Where(g => g.Start >= date && g.Start < nextDay && g.League == league).ToListAsync();
            var gamesCollection = new ObservableCollection<Game>(gamesList);
            return gamesCollection;
        }

        [HttpGet("LastGames")]
        public async Task<string> GetLastThreeGames(string teamName)
        {
            var gameResults = await _context.Games.Where(g => (g.HomeTeamName.ToLower() == teamName.ToLower() || g.AwayTeamName.ToLower() == teamName.ToLower()) && g.Ended)
                .OrderByDescending(g => g.Start).Take(3).Select(g => DetermineGameResult(g, teamName.ToLower())).ToListAsync();
            gameResults.Reverse();
            return string.Join(" ", gameResults);
        }

        [HttpGet("LastGamesForAll")]
        public async Task<ActionResult<Dictionary<string, List<string>>>> GetLastGamesForAll()
        {
            var games = await _context.Games
                .Where(g => g.Ended)
                .OrderByDescending(g => g.Start).ToListAsync();
            var groupedGames = games.GroupBy(g => g.HomeTeamName).Union(games.GroupBy(g => g.AwayTeamName))
                .GroupBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.SelectMany(gg => gg).OrderByDescending(gg => gg.Start).Take(3).Reverse()
                .Select(gg => DetermineGameResult(gg, g.Key)).ToList());
            return Ok(groupedGames);
        }

        [HttpGet("GetRoster")]
        public async Task<ActionResult<IEnumerable<GameRoster>>> GetGameRoster(string hometeam, string awayteam, DateTime gameDate)
        {
            if(hometeam.ToLower() == "kaerpaet")
            {
                hometeam = "Kärpät";
            }
            else if(awayteam.ToLower() == "kaerpaet")
            {
                awayteam = "Kärpät";
            }
            if(hometeam.ToLower() == "aessaet")
            {
                hometeam = "Ässät";
            }
            else if(awayteam.ToLower() == "aessaet")
            {
                awayteam = "Ässät";
            }
            if(hometeam.ToLower() == "kiekko-Espoo")
            {
                hometeam = "K-Espoo";
            }
            else if(awayteam.ToLower() == "kiekko-espoo")
            {
                awayteam = "K-Espoo";
            }
            var liigaGame = await _context.Games.Where(g => g.HomeTeamName.ToLower() == hometeam.ToLower() &&
                                                           g.AwayTeamName.ToLower() == awayteam.ToLower() &&
                                                            g.Start.Date == gameDate.Date).FirstOrDefaultAsync();
            var apiUrl = liigaBaseAddress + $"games/{liigaGame.Season}/{liigaGame.Id}";
            var response = await _httpClient.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, "Error fetching data from liiga-API");
            }
            var json = await response.Content.ReadAsStringAsync();
            var gameStats = JsonConvert.DeserializeObject<GameRoster>(json);
            if (gameStats == null)
            {
                return NotFound("No gamestats available");
            }
            return Ok(gameStats);
        }
       
        [AllowAnonymous]
        [HttpPost("UpdateLiigaGameStats")]
        public async Task<ActionResult<Dictionary<int, PlayerAggregatedStats>>> UpdateGameStats(int season, int gameId)
        {
            var apiUrl = liigaBaseAddress + $"games/stats/{season}/{gameId}";                  
            var response = await _httpClient.GetAsync(apiUrl);

            var chosenGame = await _context.Games.Where(g => g.Id == gameId).FirstOrDefaultAsync();
            var gameApiUrl = liigaBaseAddress + $"games/{season}/{gameId}";
            var gameResponse = await _httpClient.GetAsync(gameApiUrl);

            if (!response.IsSuccessStatusCode || !gameResponse.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, "Error fetching data from liiga-API");
            }  
            try
            {
                var json = await gameResponse.Content.ReadAsStringAsync();
                var game = JsonConvert.DeserializeObject<GameRoster>(json);
                if (game == null)
                {
                    return NotFound("No gamegoals available");
                }
                else
                {
                    game.Game.Ended = true;
                }
                if(!game.Game.Ended)
                {
                    return BadRequest("Game has not yet ended. Cannot update stats!");
                }
                else if(game.Game.Ended)
                {
                    chosenGame.HomeTeamGoals = game.Game.HomeTeam.Goals;
                    chosenGame.AwayTeamGoals = game.Game.AwayTeam.Goals;
                    chosenGame.Ended = true;
                    await _context.SaveChangesAsync();
                }
            }
            catch
            {
                return StatusCode((int)response.StatusCode, "Error updating game goals");
            }
            try
            {
                var json = await response.Content.ReadAsStringAsync();
                var gameStats = _gameStatsService.DeserializeJson(json);
                await _gameStatsService.UpdateFantasyTeamStatsAsync(gameStats.AwayTeam.SelectMany(t => t.PeriodPlayerStats).ToList());
                await _gameStatsService.UpdateFantasyTeamStatsAsync(gameStats.HomeTeam.SelectMany(t => t.PeriodPlayerStats).ToList());
                await _gameStatsService.UpdatePlayerStatsAsync(gameStats.AwayTeam.SelectMany(t => t.PeriodPlayerStats).ToList());
                await _gameStatsService.UpdatePlayerStatsAsync(gameStats.HomeTeam.SelectMany(t => t.PeriodPlayerStats).ToList());
                return Ok();
            }            
            catch
            {
                return StatusCode((int)response.StatusCode, "Error updating player stats");
            }           
        }

        [AllowAnonymous]
        [HttpGet("GetHockeyGamesByDate")]
        public async Task<IActionResult> GetHockeyGamesByDate(DateTime date)
        {
            var targetDate = date.ToString("yyyyMMdd");
            var events = await _context.Events.Where(e => e.EventStartDate.ToString().Substring(0, 8) == targetDate)
                .Include(e => e.HomeTeam)
                .Include(e => e.AwayTeam)
                .ToListAsync();
            var jsonSettings = new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                Formatting = Formatting.Indented
            };
            var jsonResponse = JsonConvert.SerializeObject(events, jsonSettings);
            return new ContentResult
            {
                Content = jsonResponse,
                ContentType = "application/json"
            };
        }
       
        [AllowAnonymous]
        [HttpPost("PopulateHockeyGamesByLeague")]
        public async Task<ActionResult> PopulateHockeyGames(string league, string country, int season)
        {
            LSGame requestedGameObject;
            var apiUrl = $"{liveScoreBaseAddress}matches/v2/list-by-league?Category=hockey&Ccd={country}&Scd={league}&Timezone=2";
            var leagueId = await _context.Series.Where(s => s.SerieName.ToLower() == league.ToLower()).Select(s => s.SerieId).FirstOrDefaultAsync();
            try
            {
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(apiUrl),
                    Headers =
                    {
                        { "x-rapidapi-key", _rapidKey },
                        { "x-rapidapi-host",  _rapidHost },
                    },
                };

                using (var response = await _httpClient.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();
                    // Deserialize JSON response to HockeyGame model using Newtonsoft.Json
                    requestedGameObject = JsonConvert.DeserializeObject<LSGame>(body);
                }
                var Id = 1;
                var LatestGameId = await _context.Games.OrderByDescending(g => g.GameId).Select(g => g.GameId).FirstOrDefaultAsync();
                foreach (var stage in requestedGameObject.Stages)
                {                    
                    foreach (var evt in stage.Events)
                    {
                        var homeTeamId = Guid.NewGuid();                                            
                        if (!string.IsNullOrEmpty(evt.T1[0].ID) && Guid.TryParse(evt.T1[0].ID, out var parsedHomeTeamId))
                        {
                            homeTeamId = parsedHomeTeamId;
                        }
                        var awayTeamId = Guid.NewGuid();
                        if (!string.IsNullOrEmpty(evt.T2[0].ID) && Guid.TryParse(evt.T2[0].ID, out var parsedAwayTeamId))
                        {
                            awayTeamId = parsedAwayTeamId;
                        }                       
                        var newGame = new Game
                        {
                            Id = Id,
                            Season = season,
                            Start = TimestampConverter.ConvertToDateTime(evt.Esd),                                 
                            HomeTeamName = evt.T1[0].Nm,
                            AwayTeamName = evt.T2[0].Nm,                            
                            AwayTeamGoals = 0,
                            HomeTeamGoals = 0,                                                       
                            FinishedType = "ACTIVE_OR_NOT_STARTED",
                            Started = false,
                            Ended = false,                          
                            Serie = "REGULAR_SEASON",
                            LastUpdated = DateTime.UtcNow,
                            League = leagueId,
                        };
                        Id++;
                        await _context.Games.AddAsync(newGame);
                    }
                }
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (HttpRequestException e)
            {
                // Handle network-related errors
                Console.WriteLine($"Request error: {e.Message}");
                throw;
            }
            catch (Exception e)
            {
                // Handle other potential errors
                Console.WriteLine($"An error occurred: {e.Message}");
                throw;
            }
        }

        [AllowAnonymous]
        [HttpPost("PopulateLiigaGamesByYear", Name = "PopulateLiigaGames")]
        public async Task<ActionResult> PopulateLiigaGames(int year)
        {
            var apiUrl = liigaBaseAddress + $"games?tournament=runkosarja&season={year}";
            var response = await _httpClient.GetAsync(apiUrl);
            var leagueId = await _context.Series.Where(s => s.SerieName.ToLower() == "liiga").Select(x => x.SerieId).FirstOrDefaultAsync();
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
                .FirstOrDefaultAsync(g => g.Id == gameDto.Id);

                if (existingGame != null)
                {
                    // Update existing game
                    existingGame.Season = gameDto.Season;
                    existingGame.Start = gameDto.Start;
                    existingGame.FinishedType = gameDto.FinishedType;
                    existingGame.Started = gameDto.Started;
                    existingGame.Ended = gameDto.Ended;
                    existingGame.HomeTeamGoals = gameDto.HomeTeam.Goals;
                    existingGame.AwayTeamGoals = gameDto.AwayTeam.Goals;
                    existingGame.LastUpdated = DateTime.UtcNow;
                   
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
                        HomeTeamName = gameDto.HomeTeam.TeamName,
                        HomeTeamGoals = gameDto.HomeTeam.Goals,
                        AwayTeamName = gameDto.AwayTeam.TeamName,
                        AwayTeamGoals = gameDto.AwayTeam.Goals,                      
                        FinishedType = gameDto.FinishedType,
                        Started = gameDto.Started,
                        Ended = gameDto.Ended,
                        Serie = gameDto.Serie,
                        LastUpdated = DateTime.UtcNow,
                        League = leagueId
                    };
                    await _context.Games.AddAsync(newGame);
                }
            }
            await _context.SaveChangesAsync();
            return Ok("Games data populated successfully");
        }

        private static string DetermineGameResult(Game game, string teamName)
        {
            bool isHomeTeam = game.HomeTeamName.ToLower() == teamName.ToLower();
            bool won = isHomeTeam ? game.HomeTeamGoals > game.AwayTeamGoals
                : game.AwayTeamGoals > game.HomeTeamGoals;
            return won ? "W" : "L";
        }
    }
}

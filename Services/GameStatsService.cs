using SporttiporssiAPI.Models.DBModels;
using Newtonsoft.Json;
using SporttiporssiAPI.Interfaces;
using Microsoft.EntityFrameworkCore;
using SporttiporssiAPI.Models;

namespace SporttiporssiAPI.Services
{
    public class GameStatsService : IGameStatsService
    {
        private readonly ApplicationDbContext _context;
        public GameStatsService(ApplicationDbContext context) { _context = context; }
        public GameStats DeserializeJson(string json)
        {
            return JsonConvert.DeserializeObject<GameStats>(json);
        }

        public Dictionary<int, PlayerAggregatedStats> AggregatePlayerStats(List<PlayerStats> playerStatsList)
        {
            var aggregatedStats = new Dictionary<int, PlayerAggregatedStats>();
            foreach(var stat in playerStatsList)
            {
                var player = _context.Players.Where(p => p.PlayerId == stat.PlayerId).FirstOrDefault();
                if(!aggregatedStats.ContainsKey(stat.PlayerId))
                {
                    if(player != null)
                    {
                        aggregatedStats[stat.PlayerId] = new PlayerAggregatedStats
                        {
                            TimeOfIce = 0,
                            GamesPlayed = player.PlayedGames
                        };
                    }
                    else
                    {
                        aggregatedStats[stat.PlayerId] = new PlayerAggregatedStats
                        {
                            TimeOfIce = 0,
                            GamesPlayed = 0
                        };
                    }                   
                }
                var aggregated = aggregatedStats[stat.PlayerId];
                var period = stat.Period;

                aggregated.Points += period.Points;
                aggregated.Assists += period.Assists;
                aggregated.Goals += period.Goals;
                aggregated.Shots += period.Shots;
                aggregated.PenaltyMinutes += period.PenaltyMinutes;
                aggregated.TimeOfIce += period.TimeOfIce;
                aggregated.CorsiFor += period.CorsiFor;
                aggregated.CorsiAgainst += period.CorsiAgainst;
                aggregated.Distance += (int)Math.Round(stat.Distance);
                aggregated.TotalPasses += stat.TotalPasses;
                aggregated.SuccessfulPasses += stat.SuccessfulPasses;
                aggregated.ExpectedGoalsPlayer += stat.ExpectedGoalsPlayer;
                aggregated.ExpectedGoalsTeam += stat.ExpectedGoalsTeam;
                aggregated.ExpectedGoalsAgainst += stat.ExpectedGoalsAgainst;
                aggregated.PlusMinus += period.PlusMinus;
                aggregated.BlockedShots += period.BlockedShots;
                aggregated.FaceoffsWon += period.FaceoffsWon;
                aggregated.FaceoffsTotal += period.FaceoffsTotal;

                // Increment GamesPlayed only if this game has not been processed yet
                if (!aggregated.ProcessedGames.Contains(stat.JerseyId))
                {
                    aggregated.GamesPlayed++;
                    aggregated.ProcessedGames.Add(stat.JerseyId);
                }
            }
            return aggregatedStats;
        }

        public async Task UpdateFantasyTeamStatsAsync(List<PlayerStats> PlayerStatsList)
        {
            var aggregatedStats = AggregatePlayerStats(PlayerStatsList);

            var fantasyTeamPlayerLinks = await _context.FantasyTeamPlayerLinks.ToListAsync();
            var fantasyGroups = await _context.FantasyGroups.ToListAsync();
            var fantasyGroupTeamLinks = await _context.FantasyGroupTeamLinks.ToListAsync();

            var fantasyTeamStatsDict = new Dictionary<Guid, FantasyTeamStats>();

            foreach (var link in fantasyTeamPlayerLinks)
            {
                if (aggregatedStats.ContainsKey(link.PlayerId))
                {
                    if (!fantasyTeamStatsDict.ContainsKey(link.FantasyTeamId))
                    {
                        var existingStats = await _context.FantasyTeamStats
                            .FirstOrDefaultAsync(f => f.FantasyTeamId == link.FantasyTeamId);

                        if (existingStats == null)
                        {
                            existingStats = new FantasyTeamStats
                            {
                                FTPId = Guid.NewGuid(),
                                FantasyTeamId = link.FantasyTeamId,
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

                            _context.FantasyTeamStats.Add(existingStats);
                        }

                        fantasyTeamStatsDict[link.FantasyTeamId] = existingStats;
                    }

                    var playerStats = aggregatedStats[link.PlayerId];
                    var fantasyTeamStats = fantasyTeamStatsDict[link.FantasyTeamId];
                    var playerCalc = await _context.Players.FirstOrDefaultAsync(p => p.PlayerId == link.PlayerId);
                    var groupLink = fantasyGroupTeamLinks.FirstOrDefault(g => g.FantasyTeamId == link.FantasyTeamId);
                    if (groupLink != null)
                    {
                        var group = fantasyGroups.FirstOrDefault(g => g.GroupId == groupLink.GroupId);                        
                        if (group != null)
                        {
                            float playerFTP = 0;
                            switch (playerCalc.Role)
                            {
                                case "OL":
                                case "VL":
                                case "KH":
                                    playerFTP += playerStats.Goals * group.OffenceGoalFTP;
                                    playerFTP += playerStats.Assists * group.OffencePassFTP;                                   
                                    playerFTP += playerStats.Shots * group.OffenceShotFTP;
                                    playerFTP += playerStats.BlockedShots * group.BlockedShotFTP;
                                    playerFTP += playerStats.PlusMinus * group.OffencePowerFTP;
                                    break;
                                case "VP":
                                case "OP":
                                    playerFTP += playerStats.Goals * group.DefenceGoalFTP;
                                    playerFTP += playerStats.Assists * group.DefencePassFTP;
                                    playerFTP += playerStats.Shots * group.DefenceShotFTP;
                                    playerFTP += playerStats.BlockedShots * group.BlockedShotFTP;
                                    playerFTP += playerStats.PlusMinus * group.DefencePowerFTP;
                                    break;
                                case "MV":
                                    playerFTP += playerStats.Goals * group.GoalieGoalFTP;
                                    playerFTP += playerStats.Assists * group.GoaliePassFTP;
                                    playerFTP += playerStats.Saves * group.GoalieSaveFTP;
                                    break;
                            }
                            playerFTP += playerStats.FaceoffsWon * (int)group.FaceOffFTP;
                            fantasyTeamStats.TotalFTP += (int)playerFTP;

                            // Update the player's FTP                            
                            //if (playerCalc != null)
                            //{
                            //    playerCalc.FTP += (int)playerFTP;
                            //}
                        }
                    }

                    // Update fantasyteamstats
                    fantasyTeamStats.Goals += playerStats.Goals;
                    fantasyTeamStats.Assists += playerStats.Assists;
                    fantasyTeamStats.Shots += playerStats.Shots;
                    fantasyTeamStats.PenaltyMinutes += playerStats.PenaltyMinutes;
                    fantasyTeamStats.FaceoffWins += playerStats.FaceoffsWon;
                    fantasyTeamStats.PlusMinus += playerStats.PlusMinus;
                    fantasyTeamStats.BlockedShots += playerStats.BlockedShots;
                    fantasyTeamStats.Distance += (int)playerStats.Distance;
                    fantasyTeamStats.Saves += (int)playerStats.Saves;                    
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdatePlayerStatsAsync(List<PlayerStats> playerStatsList)
        {
            var aggregatedStats = AggregatePlayerStats(playerStatsList);
            var hockeyDefaultFTP = await _context.HockeyDefaultFTPs.FirstOrDefaultAsync();
            foreach (var playerId in aggregatedStats.Keys)
            {
                var playerStats = aggregatedStats[playerId];
                var player = await _context.Players.FirstOrDefaultAsync(p => p.PlayerId == playerId);

                if (player != null)
                {
                    player.Goals += playerStats.Goals;
                    player.Assists += playerStats.Assists;
                    player.Points += playerStats.Points;
                    player.Shots += playerStats.Shots;
                    player.PenaltyMinutes += playerStats.PenaltyMinutes;
                    player.TimeOnIce += playerStats.TimeOfIce;
                    player.PlusMinus += playerStats.PlusMinus;
                    player.BlockedShots += playerStats.BlockedShots;
                    player.FaceoffsWon += playerStats.FaceoffsWon;
                    player.FaceoffsTotal += playerStats.FaceoffsTotal;
                    player.FaceoffsLost = (playerStats.FaceoffsTotal - playerStats.FaceoffsWon);
                    player.PlayedGames += playerStats.GamesPlayed;
                    player.LastUpdated = DateTime.Now;

                    // Calculate averages
                    player.TimeOnIceAvg = TimeSpan.FromSeconds(playerStats.TimeOfIce / playerStats.GamesPlayed).ToString(@"mm\:ss");
                    player.FaceoffWonPercentage = playerStats.FaceoffsTotal > 0 ? (float)playerStats.FaceoffsWon / playerStats.FaceoffsTotal * 100 : 0;
                    player.ShotPercentage = playerStats.Shots > 0 ? (float)playerStats.Goals / playerStats.Shots * 100 : 0;

                    switch (player.Role)
                    {
                        case "OL":
                        case "VL":
                        case "KH":
                            player.FTP += playerStats.Goals * hockeyDefaultFTP.OffenceGoalFTP;
                            player.FTP += playerStats.Assists * hockeyDefaultFTP.OffencePassFTP;
                            player.FTP += (int)(playerStats.Shots * hockeyDefaultFTP.OffenceShotFTP);
                            player.FTP += playerStats.BlockedShots * hockeyDefaultFTP.BlockedShotFTP;
                            player.FTP += (int)(playerStats.PlusMinus * hockeyDefaultFTP.OffencePowerFTP);
                            break;
                        case "VP":
                        case "OP":
                            player.FTP += playerStats.Goals * hockeyDefaultFTP.DefenceGoalFTP;
                            player.FTP += playerStats.Assists * hockeyDefaultFTP.DefencePassFTP;
                            player.FTP += (int)(playerStats.Shots * hockeyDefaultFTP.DefenceShotFTP);
                            player.FTP += playerStats.BlockedShots * hockeyDefaultFTP.BlockedShotFTP;
                            player.FTP += (int)(playerStats.PlusMinus * hockeyDefaultFTP.DefencePowerFTP);
                            break;                       
                    }

                }
            }
            await _context.SaveChangesAsync();
        }        
    }
}

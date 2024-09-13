using SporttiporssiAPI.Models.DBModels;

namespace SporttiporssiAPI.Interfaces
{
    public interface IGameStatsService
    {
        GameStats DeserializeJson(string json);

        Dictionary<int, PlayerAggregatedStats> AggregatePlayerStats(List<PlayerStats> playerStatsList);

        Task UpdateFantasyTeamStatsAsync(List<PlayerStats> PlayerStatsList);

        Task UpdatePlayerStatsAsync(List<PlayerStats> playerStatsList);
    }
}

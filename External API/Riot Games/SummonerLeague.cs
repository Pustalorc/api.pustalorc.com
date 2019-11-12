namespace api.pustalorc.xyz.External_API.Riot_Games
{
    public class SummonerLeague
    {
        public string LeagueId { get; set; }
        public string QueueType { get; set; }
        public string Tier { get; set; }
        public string Rank { get; set; }
        public string SummonerId { get; set; }
        public string SummonerName { get; set; }
        public int LeaguePoints { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public bool Veteran { get; set; }
        public bool Inactive { get; set; }
        public bool FreshBlood { get; set; }
        public bool HotStreak { get; set; }
    }
}
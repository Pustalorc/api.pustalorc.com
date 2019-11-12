using System.Collections.Generic;

namespace api.pustalorc.xyz.JSON_Classes
{
    public class LeagueOfLegendsTeam : TournamentTeam
    {
        public string AverageRank { get; set; } = "";
        public List<LeagueOfLegendsPlayer> Players { get => Members.ConvertAll(k => k as LeagueOfLegendsPlayer); set => Members = value.ConvertAll(k => k as TeamPlayer); }
    }
}
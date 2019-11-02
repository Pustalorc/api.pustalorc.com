﻿using System.Collections.Generic;

namespace api.pustalorc.xyz.JSON_Classes
{
    public class LeagueOfLegendsTeam
    {
        public string TournamentName { get; set; } = "";
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string AverageRank { get; set; } = "";
        public List<LeagueOfLegendsPlayer> Members { get; set; } = new List<LeagueOfLegendsPlayer>();
    }
}
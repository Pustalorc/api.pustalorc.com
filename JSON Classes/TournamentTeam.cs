using api.pustalorc.xyz.Configuration;
using System.Collections.Generic;

namespace api.pustalorc.xyz.JSON_Classes
{
    public abstract class TournamentTeam
    {
        public Tournament Tournament { get; set; } = null;
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public List<TeamPlayer> Members { get; set; } = new List<TeamPlayer>();
    }
}
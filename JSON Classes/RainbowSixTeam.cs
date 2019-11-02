using System.Collections.Generic;

namespace api.pustalorc.xyz.JSON_Classes
{
    public class RainbowSixTeam
    {
        public string TournamentName { get; set; } = "";
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public int AverageMmr { get; set; } = 0;
        public List<RainbowSixPlayer> Members { get; set; } = new List<RainbowSixPlayer>();
    }
}
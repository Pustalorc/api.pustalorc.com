using System.Collections.Generic;

namespace api.pustalorc.xyz.JSON_Classes
{
    public class RainbowSixTeam : TournamentTeam
    {
        public int AverageMmr { get; set; } = 0;

        public List<RainbowSixPlayer> Players
        {
            get => Members.ConvertAll(k => k as RainbowSixPlayer);
            set => Members = value.ConvertAll(k => k as TeamPlayer);
        }
    }
}
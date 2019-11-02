namespace api.pustalorc.xyz.Configuration
{
    public class Tournament
    {
        public ETournamentType TournamentType { get; set; }
        public string TournamentName { get; set; }
        public string FriendlyName { get; set; }

        public Tournament(ETournamentType type, string name, string friendlyName)
        {
            TournamentType = type;
            TournamentName = name;
            FriendlyName = friendlyName;
        }
    }
}
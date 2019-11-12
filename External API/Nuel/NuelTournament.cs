namespace api.pustalorc.xyz.External_API.Nuel
{
    public class NuelTournament
    {
        public Game game { get; set; }
        public string name { get; set; }
        public string summary { get; set; }
        public About about { get; set; }
        public Prizes prizes { get; set; }
        public Rules rules { get; set; }
        public Theme theme { get; set; }
        public Splash splash { get; set; }
        public Schedule[] schedule { get; set; }
        public Social social { get; set; }
    }
}
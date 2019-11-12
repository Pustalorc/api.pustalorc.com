namespace api.pustalorc.xyz.External_API.Nuel
{
    public class NuelTournament
    {
        public Game Game { get; set; }
        public string Name { get; set; }
        public string Summary { get; set; }
        public About About { get; set; }
        public Prizes Prizes { get; set; }
        public Rules Rules { get; set; }
        public Theme Theme { get; set; }
        public Splash Splash { get; set; }
        public Schedule[] Schedule { get; set; }
        public Social Social { get; set; }
    }
}
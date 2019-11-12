namespace api.pustalorc.xyz.External_API.Nuel
{
    public class TournamentSchedule
    {
        public string Name { get; set; }
        public Criteria Criteria { get; set; }
        public Team[] Teams { get; set; }
    }
}
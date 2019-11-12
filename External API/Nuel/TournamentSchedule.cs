namespace api.pustalorc.xyz.External_API.Nuel
{
    public class TournamentSchedule
    {
        public string name { get; set; }
        public Criteria criteria { get; set; }
        public Team[] teams { get; set; }
    }
}
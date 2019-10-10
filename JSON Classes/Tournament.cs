namespace api.pustalorc.xyz.JSON_Classes
{
    public class Tournament
    {
        public string name { get; set; }
        public Criteria criteria { get; set; }
        public Team[] teams { get; set; }
    }
}
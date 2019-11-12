namespace api.pustalorc.xyz.External_API.Nuel
{
    public class Team
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public University University { get; set; }
        public Eligibility Eligibility { get; set; }
        public string CaptainUserId { get; set; }
        public Member[] Members { get; set; }
    }
}
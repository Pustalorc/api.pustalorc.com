namespace api.pustalorc.xyz.External_API.Nuel
{
    public class Team
    {
        public string id { get; set; }
        public string name { get; set; }
        public University university { get; set; }
        public Eligibility eligibility { get; set; }
        public string captainUserId { get; set; }
        public Member[] members { get; set; }
    }
}
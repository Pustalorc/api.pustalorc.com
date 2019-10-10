namespace api.pustalorc.xyz.JSON_Classes
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
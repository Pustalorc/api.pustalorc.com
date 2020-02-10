namespace api.pustalorc.xyz
{
    public static class LeagueUtils
    {
        public static string FromIntToTierAndRank(int val)
        {
            return val switch
            {
                1 => "iron_1",
                2 => "iron_2",
                3 => "iron_3",
                4 => "iron_4",
                5 => "bronze_1",
                6 => "bronze_2",
                7 => "bronze_3",
                8 => "bronze_4",
                9 => "silver_1",
                10 => "silver_2",
                11 => "silver_3",
                12 => "silver_4",
                13 => "gold_1",
                14 => "gold_2",
                15 => "gold_3",
                16 => "gold_4",
                17 => "platinum_1",
                18 => "platinum_2",
                19 => "platinum_3",
                20 => "platinum_4",
                21 => "diamond_1",
                22 => "diamond_2",
                23 => "diamond_3",
                24 => "diamond_4",
                25 => "master_1",
                26 => "grandmaster_1",
                27 => "challenger_1",
                _ => "bronze_1"
            };
        }

        public static int FromTierToInt(string tier)
        {
            return tier.ToLower() switch
            {
                "iron" => 0,
                "bronze" => 4,
                "silver" => 8,
                "gold" => 12,
                "platinum" => 16,
                "diamond" => 20,
                "master" => 24,
                "grandmaster" => 25,
                "challenger" => 26,
                _ => 5
            };
        }

        public static int FromRomanToInt(string rank)
        {
            return rank.ToLower() switch
            {
                "i" => 1,
                "ii" => 2,
                "iii" => 3,
                "iv" => 4,
                _ => 1
            };
        }
    }
}
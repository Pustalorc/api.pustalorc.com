namespace api.pustalorc.xyz
{
    public static class LeagueUtils
    {
        public static string FromIntToTierAndRank(int val)
        {
            switch (val)
            {
                case 1:
                    return "iron_1";
                case 2:
                    return "iron_2";
                case 3:
                    return "iron_3";
                case 4:
                    return "iron_4";
                case 5:
                    return "bronze_1";
                case 6:
                    return "bronze_2";
                case 7:
                    return "bronze_3";
                case 8:
                    return "bronze_4";
                case 9:
                    return "silver_1";
                case 10:
                    return "silver_2";
                case 11:
                    return "silver_3";
                case 12:
                    return "silver_4";
                case 13:
                    return "gold_1";
                case 14:
                    return "gold_2";
                case 15:
                    return "gold_3";
                case 16:
                    return "gold_4";
                case 17:
                    return "platinum_1";
                case 18:
                    return "platinum_2";
                case 19:
                    return "platinum_3";
                case 20:
                    return "platinum_4";
                case 21:
                    return "diamond_1";
                case 22:
                    return "diamond_2";
                case 23:
                    return "diamond_3";
                case 24:
                    return "diamond_4";
                case 25:
                    return "master_1";
                case 26:
                    return "grandmaster_1";
                case 27:
                    return "challenger_1";
                default:
                    return "bronze_1";
            }
        }

        public static int FromTierToInt(string tier)
        {
            switch (tier.ToLower())
            {
                case "iron":
                    return 0;
                case "bronze":
                    return 4;
                case "silver":
                    return 8;
                case "gold":
                    return 12;
                case "platinum":
                    return 16;
                case "diamond":
                    return 20;
                case "master":
                    return 24;
                case "grandmaster":
                    return 25;
                case "challenger":
                    return 26;
                default:
                    return 5;
            }
        }

        public static int FromRomanToInt(string rank)
        {
            switch (rank.ToLower())
            {
                case "i":
                    return 1;
                case "ii":
                    return 2;
                case "iii":
                    return 3;
                case "iv":
                    return 4;
                default:
                    return 1;
            }
        }
    }
}
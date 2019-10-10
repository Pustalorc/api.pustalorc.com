using System.Collections.Generic;

namespace api.pustalorc.xyz.JSON_Classes
{
    public class SimplePlayer
    {
        public string Name { get; set; } = "";
        public string ProfilePicture { get; set; } = "";
        public string Rank { get; set; } = "";
        public int MMR { get; set; } = 0;
    }
}
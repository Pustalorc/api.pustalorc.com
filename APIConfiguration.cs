using Newtonsoft.Json;
using System;
using System.IO;

namespace api.pustalorc.xyz
{
    public class APIConfiguration
    {
        [JsonIgnore] public static string FileName { get; } = "Config/configuration.json";

        public string LoLApiKey { get; set; } = Guid.Empty.ToString();
        public string LolTournamentName { get; set; } = "league-of-legends-university-series-winter-2019";
        public string R6STournamentName { get; set; } = "rainbow-six-siege-university-league-winter-2019";
        public string NuelTournamentAPI { get; set; } = "https://tournament-cms.dev.thenuel.com/";
        public string NuelSignupPoolsAPI { get; set; } = "https://teams.dev.thenuel.com/signup-pools/";

        public void SaveJson()
        {
            var file = Path.Combine(AppContext.BaseDirectory, FileName);
            File.WriteAllText(file, ToJson());
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public static void EnsureExists()
        {
            var file = Path.Combine(AppContext.BaseDirectory, FileName);
            if (!File.Exists(file))
            {
                var path = Path.GetDirectoryName(file);
                if (!Directory.Exists(path) && path != null)
                    Directory.CreateDirectory(path);

                var config = new APIConfiguration();

                config.SaveJson();
                return;
            }
        }

        public static APIConfiguration Load()
        {
            var file = Path.Combine(AppContext.BaseDirectory, FileName);
            EnsureExists();
            return JsonConvert.DeserializeObject<APIConfiguration>(File.ReadAllText(file));
        }
    }
}
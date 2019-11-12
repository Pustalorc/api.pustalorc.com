using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace api.pustalorc.xyz.Configuration
{
    public class ApiConfiguration
    {
        [JsonIgnore] public static string FileName { get; } = "Config/configuration.json";

        public string RiotApiKey { get; set; } = Guid.Empty.ToString();
        public string NuelTournamentApi { get; set; } = "https://tournament-cms.dev.thenuel.com/";
        public string NuelSignupPoolsApi { get; set; } = "https://teams.dev.thenuel.com/signup-pools/";

        public List<Tournament> Tournaments { get; set; } = new List<Tournament>();

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
            if (File.Exists(file)) return;

            var path = Path.GetDirectoryName(file);
            if (!Directory.Exists(path) && path != null)
                Directory.CreateDirectory(path);

            var config = new ApiConfiguration();

            config.SaveJson();
        }

        public static ApiConfiguration Load()
        {
            var file = Path.Combine(AppContext.BaseDirectory, FileName);
            EnsureExists();
            return JsonConvert.DeserializeObject<ApiConfiguration>(File.ReadAllText(file));
        }
    }
}
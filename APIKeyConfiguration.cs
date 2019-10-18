using Newtonsoft.Json;
using System;
using System.IO;

namespace api.pustalorc.xyz
{
    public class APIKeyConfiguration
    {
        [JsonIgnore] public static string FileName { get; } = "Config/configuration.json";

        public string LoLApiKey { get; set; } = Guid.Empty.ToString();

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

                var config = new APIKeyConfiguration();

                config.SaveJson();
                return;
            }
        }

        public static APIKeyConfiguration Load()
        {
            var file = Path.Combine(AppContext.BaseDirectory, FileName);
            EnsureExists();
            return JsonConvert.DeserializeObject<APIKeyConfiguration>(File.ReadAllText(file));
        }
    }
}
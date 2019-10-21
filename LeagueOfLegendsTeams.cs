using api.pustalorc.xyz.JSON_Classes;
using api.pustalorc.xyz.JSON_Classes.External_API.Riot_Games;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using api.pustalorc.xyz.JSON_Classes.External_API.Nuel;
using System;

namespace api.pustalorc.xyz
{
    public static class LeagueOfLegendsTeams
    {
        public static List<LeagueOfLegendsTeam> Teams = new List<LeagueOfLegendsTeam>();

        public static void RetrieveGroups()
        {
            var configuration = ApiConfiguration.Load();
            var finalTeams = new List<LeagueOfLegendsTeam>();

            using (var web = new WebClient())
            {
                var teams = new List<Team>();
                var profileIcons = JsonConvert.DeserializeObject<ProfileIcons>(web.DownloadString("http://ddragon.leagueoflegends.com/cdn/9.20.1/data/en_US/profileicon.json"));

                foreach (var team in JsonConvert
                    .DeserializeObject<NuelTournament>(web.DownloadString(configuration.NuelTournamentApi + configuration.LolTournamentName))
                    .schedule.Where(k => k.isPlayableWeek && DateTime.UtcNow < DateTime.Parse(k.date)).ToList().ConvertAll(k => k.tournamentId).Select(id =>
                        JsonConvert.DeserializeObject<Tournament>(
                            web.DownloadString(configuration.NuelSignupPoolsApi + id)))
                    .Where(team => team.teams.Any()))
                    teams.AddRange(team.teams);

                foreach (var team in teams)
                {
                    var players = new List<LeagueOfLegendsPlayer>();
                    var teamRank = 0;

                    foreach (var player in team.members.ToList())
                        if (player.inGameName?.displayName == null)
                        {
                            players.Add(new LeagueOfLegendsPlayer
                            {
                                Name = player.userId, IsCaptain = player.userId == team.captainUserId
                            });
                            teamRank += 5;
                        }
                        else
                            try
                            {
                                var data = web.DownloadString(
                                    $"https://euw1.api.riotgames.com/lol/summoner/v4/summoners/by-name/{player.inGameName.displayName}?api_key={configuration.LoLApiKey}");
                                var summonerDetails = JsonConvert.DeserializeObject<Summoner>(data);
                                var data2 = web.DownloadString(
                                    $"https://euw1.api.riotgames.com/lol/league/v4/entries/by-summoner/{summonerDetails.id}?api_key={configuration.LoLApiKey}");
                                var playerStats = JsonConvert.DeserializeObject<SummonerLeague[]>(data2);
                                Thread.Sleep(2500);

                                if ((playerStats?.Length ?? 0) > 0)
                                {
                                    var tftData = playerStats.FirstOrDefault(k =>
                                        k.queueType.Equals("RANKED_TFT",
                                            StringComparison.InvariantCultureIgnoreCase));
                                    var soloRankedData = playerStats.FirstOrDefault(k =>
                                        k.queueType.Equals("RANKED_SOLO_5x5",
                                            StringComparison.InvariantCultureIgnoreCase));
                                    players.Add(new LeagueOfLegendsPlayer
                                    {
                                        Name = player.inGameName.displayName,
                                        TftRank = tftData == null
                                            ? "bronze_1"
                                            : tftData.tier.ToLower() + "_" + FromRomanToInt(tftData.rank),
                                        Rank = soloRankedData == null
                                            ? "bronze_1"
                                            : soloRankedData.tier.ToLower() + "_" + FromRomanToInt(soloRankedData.rank),
                                        PlayerId = summonerDetails.id,
                                        ProfileIconId = int.TryParse(profileIcons.data.GetType().GetProperty("_" + summonerDetails.profileIconId)?.Name?.Substring(1) ?? "0", out var id) ? id : 0,
                                        IsCaptain = player.userId == team.captainUserId
                                    });
                                    teamRank += soloRankedData == null ? 5 : FromTierToInt(soloRankedData.tier.ToLower()) + FromRomanToInt(soloRankedData.rank);
                                }
                                else
                                {
                                    players.Add(new LeagueOfLegendsPlayer
                                    {
                                        Name = player.inGameName.displayName,
                                        IsCaptain = player.userId == team.captainUserId
                                    });
                                    teamRank += 5;
                                }
                            }
                            catch (WebException ex)
                            {
                                if (ex.Status != WebExceptionStatus.ProtocolError || ex.Response == null) throw;

                                var resp = (HttpWebResponse) ex.Response;
                                if (resp.StatusCode != HttpStatusCode.NotFound) throw;

                                players.Add(new LeagueOfLegendsPlayer
                                {
                                    Name = player.inGameName.displayName,
                                    IsCaptain = player.userId == team.captainUserId
                                });
                                teamRank += 5;
                            }

                    var final = finalTeams.FirstOrDefault(k => k.Id.Equals(team.id));
                    players = players.OrderBy(k => k.Name).ToList();

                    if (final == null)
                        finalTeams.Add(new LeagueOfLegendsTeam
                        {
                            Id = team.id,
                            Name = team.name,
                            AverageRank = FromIntToTierAndRank(teamRank / players.Count),
                            Members = players
                        });
                    else
                    {
                        final.Members = players;
                        final.AverageRank = FromIntToTierAndRank(teamRank / players.Count);
                    }
                }

                Teams = finalTeams.OrderBy(k => k.Name).ToList();
            }
        }

        private static string FromIntToTierAndRank(int val)
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

        private static int FromTierToInt(string tier)
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

        private static int FromRomanToInt(string rank)
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
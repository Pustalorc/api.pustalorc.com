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
            var configuration = APIConfiguration.Load();
            var finalTeams = new List<LeagueOfLegendsTeam>();

            using (var web = new WebClient())
            {
                var teams = new List<Team>();

                foreach (var team in JsonConvert
                    .DeserializeObject<NuelTournament>(web.DownloadString(configuration.NuelTournamentAPI + configuration.LolTournamentName))
                    .schedule.Where(k => k.isPlayableWeek && System.DateTime.UtcNow < System.DateTime.Parse(k.date)).ToList().ConvertAll(k => k.tournamentId).Select(id =>
                        JsonConvert.DeserializeObject<Tournament>(
                            web.DownloadString(configuration.NuelSignupPoolsAPI + id)))
                    .Where(team => team.teams.Any()))
                    teams.AddRange(team.teams.Where(k => k.eligibility.isEligible).ToArray());

                foreach (var team in teams)
                {
                    var players = new List<LeagueOfLegendsPlayer>();

                    foreach (var player in team.members.ToList())
                        if (player.inGameName?.displayName == null)
                            players.Add(new LeagueOfLegendsPlayer()
                                {Name = player.userId, IsCaptain = player.userId == team.captainUserId});
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
                                            ? "default"
                                            : tftData.tier.ToLower() + "_" + FromRomanToInt(tftData.rank),
                                        Rank = soloRankedData == null
                                            ? "default"
                                            : soloRankedData.tier.ToLower() + "_" + FromRomanToInt(soloRankedData.rank),
                                        PlayerID = summonerDetails.id,
                                        ProfileIconId = summonerDetails.profileIconId,
                                        IsCaptain = player.userId == team.captainUserId
                                    });
                                }
                                else
                                {
                                    players.Add(new LeagueOfLegendsPlayer
                                    {
                                        Name = player.inGameName.displayName,
                                        IsCaptain = player.userId == team.captainUserId
                                    });
                                }
                            }
                            catch (WebException ex)
                            {
                                if (ex.Status == WebExceptionStatus.ProtocolError && ex.Response != null)
                                {
                                    var resp = (HttpWebResponse) ex.Response;
                                    if (resp.StatusCode == HttpStatusCode.NotFound)
                                    {
                                        players.Add(new LeagueOfLegendsPlayer
                                        {
                                            Name = player.inGameName.displayName,
                                            IsCaptain = player.userId == team.captainUserId
                                        });
                                        continue;
                                    }
                                }

                                throw;
                            }

                    var final = finalTeams.FirstOrDefault(k => k.Id.Equals(team.id));
                    players.OrderBy(k => k.Name);

                    if (final == null)
                        finalTeams.Add(new LeagueOfLegendsTeam()
                        {
                            Id = team.id,
                            Name = team.name,
                            Members = players
                        });
                    else
                        final.Members = players;
                }

                Teams = finalTeams.OrderBy(k => k.Name).ToList();
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
                case "v":
                    return 5;
                default:
                    return 0;
            }
        }
    }
}
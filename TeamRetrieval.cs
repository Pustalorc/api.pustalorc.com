using api.pustalorc.xyz.JSON_Classes;
using api.pustalorc.xyz.JSON_Classes.External_API.Nuel;
using api.pustalorc.xyz.JSON_Classes.External_API.R6S;
using api.pustalorc.xyz.JSON_Classes.External_API.Riot_Games;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using api.pustalorc.xyz.Configuration;

namespace api.pustalorc.xyz
{
    public static class TeamRetrieval
    {
        public static List<LeagueOfLegendsTeam> LeagueTeams = new List<LeagueOfLegendsTeam>();
        public static List<RainbowSixTeam> RainbowTeams = new List<RainbowSixTeam>();

        public static void GetTeams()
        {
            GetRainbowTeams();
            GetLeagueTeams();
        }

        public static IEnumerable<Team> GetNuelTeams(string nuelTapi, string nuelSapi, string tournamentName)
        {
            var teams = new List<Team>();
            using (var web = new WebClient())
            {
                foreach (var tournament in JsonConvert
                    .DeserializeObject<NuelTournament>(web.DownloadString(nuelTapi + tournamentName))
                    .schedule.Where(k => k.isPlayableWeek && DateTime.UtcNow <= DateTime.Parse(k.date).AddDays(1))
                    .ToList()
                    .ConvertAll(k => k.tournamentId).Select(id =>
                        JsonConvert.DeserializeObject<TournamentSchedule>(
                            web.DownloadString(nuelSapi + id)))
                    .Where(team => team.teams.Any()))
                    teams.AddRange(tournament.teams);
            }

            return teams;
        }

        public static void GetRainbowTeams()
        {
            var config = ApiConfiguration.Load();

            using (var web = new WebClient())
            {
                foreach (var tourney in config.Tournaments.Where(k => k.TournamentType == ETournamentType.Rainbow6))
                {
                    var teams = GetNuelTeams(config.NuelTournamentApi, config.NuelSignupPoolsApi,
                        tourney.TournamentName);

                    foreach (var team in teams)
                    {
                        var players = new List<RainbowSixPlayer>();
                        var teamMmr = 0;

                        foreach (var player in team.members.ToList())
                            if (player.inGameName?.displayName == null)
                            {
                                players.Add(new RainbowSixPlayer
                                {
                                    Name = player.userId,
                                    Rank = 0,
                                    PlayerId = "",
                                    Mmr = 0,
                                    IsCaptain = player.userId == team.captainUserId
                                });
                                teamMmr += 2000;
                            }
                            else
                            {
                                var downloadStr = "{\"totalresults\":0}";
                                try
                                {
                                    downloadStr = web.DownloadString(
                                        $"https://r6tab.com/api/search.php?platform=uplay&search={player.inGameName.displayName}");
                                }
                                catch (WebException ex)
                                {
                                    if (ex.Status != WebExceptionStatus.Timeout) throw;
                                }

                                var playerData = JsonConvert.DeserializeObject<PlayerData>(downloadStr);

                                if ((playerData?.results?.Length ?? 0) > 0)
                                {
                                    var data = playerData.results[0];
                                    players.Add(new RainbowSixPlayer
                                    {
                                        Name = player.inGameName.displayName,
                                        Rank = data.p_currentrank,
                                        PlayerId = data.p_user,
                                        Mmr = data.p_currentmmr,
                                        IsCaptain = player.userId == team.captainUserId
                                    });
                                    teamMmr += data.p_currentmmr == 0 ? 2000 : data.p_currentmmr;
                                }
                                else
                                {
                                    players.Add(new RainbowSixPlayer
                                    {
                                        Name = player.inGameName.displayName,
                                        Rank = 0,
                                        PlayerId = "",
                                        Mmr = 0,
                                        IsCaptain = player.userId == team.captainUserId
                                    });
                                    teamMmr += 2000;
                                }
                            }

                        var final = RainbowTeams.FirstOrDefault(k => k.Id.Equals(team.id));
                        players = players.OrderBy(k => k.Name).ToList();

                        if (final == null)
                        {
                            RainbowTeams.Add(new RainbowSixTeam
                            {
                                TournamentName = tourney.TournamentName,
                                Id = team.id,
                                Name = team.name,
                                Members = players,
                                AverageMmr = teamMmr / players.Count
                            });
                        }
                        else
                        {
                            final.Members = players;
                            final.AverageMmr = teamMmr / players.Count;
                        }
                    }
                }
            }

            RainbowTeams = RainbowTeams.OrderBy(k => k.Name).ToList();
        }


        public static void GetLeagueTeams()
        {
            var configuration = ApiConfiguration.Load();

            using (var web = new WebClient())
            {
                var profileIcons = JsonConvert.DeserializeObject<ProfileIcons>(
                    web.DownloadString("http://ddragon.leagueoflegends.com/cdn/9.20.1/data/en_US/profileicon.json"));

                foreach (var tourney in configuration.Tournaments.Where(k => k.TournamentType == ETournamentType.League)
                )
                {
                    var teams = GetNuelTeams(configuration.NuelTournamentApi, configuration.NuelSignupPoolsApi,
                        tourney.TournamentName);
                    foreach (var team in teams)
                    {
                        var players = new List<LeagueOfLegendsPlayer>();
                        var teamRank = 0;

                        foreach (var player in team.members.ToList())
                            if (player.inGameName?.displayName == null)
                            {
                                players.Add(new LeagueOfLegendsPlayer
                                {
                                    Name = player.userId,
                                    IsCaptain = player.userId == team.captainUserId
                                });
                                teamRank += 5;
                            }
                            else
                            {
                            retry:
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
                                                : tftData.tier.ToLower() + "_" +
                                                  LeagueUtils.FromRomanToInt(tftData.rank),
                                            Rank = soloRankedData == null
                                                ? "bronze_1"
                                                : soloRankedData.tier.ToLower() + "_" +
                                                  LeagueUtils.FromRomanToInt(soloRankedData.rank),
                                            PlayerId = summonerDetails.id,
                                            ProfileIconId =
                                                int.TryParse(
                                                    profileIcons.data.GetType()
                                                        .GetProperty("_" + summonerDetails.profileIconId)?.Name
                                                        ?.Substring(1) ?? "0", out var id)
                                                    ? id
                                                    : 0,
                                            IsCaptain = player.userId == team.captainUserId
                                        });
                                        teamRank += soloRankedData == null
                                            ? 5
                                            : LeagueUtils.FromTierToInt(soloRankedData.tier.ToLower()) +
                                              LeagueUtils.FromRomanToInt(soloRankedData.rank);
                                    }
                                    else
                                    {
                                        players.Add(new LeagueOfLegendsPlayer
                                        {
                                            Name = player.inGameName.displayName,
                                            TftRank = "bronze_1",
                                            Rank = "bronze_1",
                                            PlayerId = summonerDetails.id,
                                            ProfileIconId =
                                                int.TryParse(
                                                    profileIcons.data.GetType()
                                                        .GetProperty("_" + summonerDetails.profileIconId)?.Name
                                                        ?.Substring(1) ?? "0", out var id)
                                                    ? id
                                                    : 0,
                                            IsCaptain = player.userId == team.captainUserId
                                        });
                                        teamRank += 5;
                                    }
                                }
                                catch (WebException ex)
                                {
                                    if (ex.Status != WebExceptionStatus.ProtocolError || ex.Response == null) throw;

                                    var resp = (HttpWebResponse) ex.Response;
                                    if (resp.StatusCode == HttpStatusCode.ServiceUnavailable || resp.StatusCode == HttpStatusCode.InternalServerError)
                                        goto retry;
                                    if (resp.StatusCode != HttpStatusCode.NotFound) throw;

                                    players.Add(new LeagueOfLegendsPlayer
                                    {
                                        Name = player.inGameName.displayName,
                                        IsCaptain = player.userId == team.captainUserId
                                    });
                                    teamRank += 5;
                                }
                            }

                        var final = LeagueTeams.FirstOrDefault(k => k.Id.Equals(team.id));
                        players = players.OrderBy(k => k.Name).ToList();

                        if (final == null)
                        {
                            LeagueTeams.Add(new LeagueOfLegendsTeam
                            {
                                TournamentName = tourney.TournamentName,
                                Id = team.id,
                                Name = team.name,
                                AverageRank = LeagueUtils.FromIntToTierAndRank(teamRank / players.Count),
                                Members = players
                            });
                        }
                        else
                        {
                            final.Members = players;
                            final.AverageRank = LeagueUtils.FromIntToTierAndRank(teamRank / players.Count);
                        }
                    }
                }
            }

            LeagueTeams = LeagueTeams.OrderBy(k => k.Name).ToList();
        }
    }
}
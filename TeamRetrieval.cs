using api.pustalorc.xyz.JSON_Classes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using api.pustalorc.xyz.Configuration;
using api.pustalorc.xyz.External_API.Nuel;
using api.pustalorc.xyz.External_API.R6S;
using api.pustalorc.xyz.External_API.Riot_Games;

namespace api.pustalorc.xyz
{
    public static class TeamRetrieval
    {
        public static List<TournamentTeam> Teams = new List<TournamentTeam>();

        public static void GetTeams()
        {
            var config = ApiConfiguration.Load();

            using (var web = new WebClient())
            {
                var profileIcons = JsonConvert.DeserializeObject<ProfileIcons>(
                    web.DownloadString("http://ddragon.leagueoflegends.com/cdn/9.20.1/data/en_US/profileicon.json"));

                foreach (var tournament in config.Tournaments)
                {
                    var participatingTeams = GetNuelTeams(config.NuelTournamentApi, config.NuelSignupPoolsApi,
                        tournament.TournamentName);

                    foreach (var team in participatingTeams)
                    {
                        var players = new List<TeamPlayer>();

                        foreach (var player in team.Members.ToList())
                            if (player.InGameName?.DisplayName == null)
                            {
                                TeamPlayer playerToAdd = null;
                                switch (tournament.TournamentType)
                                {
                                    case ETournamentType.Rainbow6:
                                        playerToAdd = new RainbowSixPlayer
                                        {
                                            Name = player.UserId, PlayerId = "",
                                            IsCaptain = player.UserId == team.CaptainUserId
                                        };
                                        break;
                                    case ETournamentType.TeamFightTactics:
                                    case ETournamentType.League:
                                        playerToAdd = new LeagueOfLegendsPlayer
                                        {
                                            Name = player.UserId, NumericRank = 5,
                                            IsCaptain = player.UserId == team.CaptainUserId
                                        };
                                        break;
                                }

                                if (playerToAdd == null)
                                    continue;

                                players.Add(playerToAdd);
                            }
                            else
                            {
                                switch (tournament.TournamentType)
                                {
                                    case ETournamentType.Rainbow6:
                                        var downloadStr = "{\"totalresults\":0}";
                                        try
                                        {
                                            downloadStr = web.DownloadString(
                                                $"https://r6tab.com/api/search.php?platform=uplay&search={player.InGameName.DisplayName}");
                                        }
                                        catch (WebException ex)
                                        {
                                            if (ex.Status != WebExceptionStatus.Timeout) throw;
                                        }

                                        var playerData = JsonConvert.DeserializeObject<PlayerData>(downloadStr);

                                        if ((playerData?.Results?.Length ?? 0) > 0)
                                        {
                                            var data = playerData.Results[0];
                                            players.Add(new RainbowSixPlayer
                                            {
                                                Name = player.InGameName.DisplayName,
                                                Rank = data.P_Currentrank,
                                                PlayerId = data.P_User,
                                                Mmr = data.P_Currentmmr,
                                                IsCaptain = player.UserId == team.CaptainUserId
                                            });
                                        }
                                        else
                                        {
                                            players.Add(new RainbowSixPlayer
                                            {
                                                Name = player.InGameName.DisplayName,
                                                PlayerId = "",
                                                IsCaptain = player.UserId == team.CaptainUserId
                                            });
                                        }

                                        break;
                                    case ETournamentType.TeamFightTactics:
                                        retryTft:
                                        try
                                        {
                                            var data = web.DownloadString(
                                                $"https://euw1.api.riotgames.com/tft/summoner/v1/summoners/by-name/{player.InGameName.DisplayName}?api_key={config.RiotApiKey}");
                                            var summonerDetails = JsonConvert.DeserializeObject<Summoner>(data);
                                            var data2 = web.DownloadString(
                                                $"https://euw1.api.riotgames.com/tft/league/v1/entries/by-summoner/{summonerDetails.Id}?api_key={config.RiotApiKey}");
                                            var playerStats = JsonConvert.DeserializeObject<SummonerLeague[]>(data2);
                                            Thread.Sleep(2500);

                                            if ((playerStats?.Length ?? 0) > 0)
                                            {
                                                var soloRankedData = playerStats.FirstOrDefault(k =>
                                                    k.QueueType.Equals("RANKED_TFT",
                                                        StringComparison.InvariantCultureIgnoreCase));
                                                players.Add(new LeagueOfLegendsPlayer
                                                {
                                                    Name = player.InGameName.DisplayName,
                                                    Rank = soloRankedData == null
                                                        ? "bronze_1"
                                                        : soloRankedData.Tier.ToLower() + "_" +
                                                          LeagueUtils.FromRomanToInt(soloRankedData.Rank),
                                                    NumericRank = soloRankedData == null
                                                        ? 5
                                                        : LeagueUtils.FromTierToInt(soloRankedData.Tier.ToLower()) +
                                                          LeagueUtils.FromRomanToInt(soloRankedData.Rank),
                                                    PlayerId = summonerDetails.Id,
                                                    ProfileIconId =
                                                        int.TryParse(
                                                            profileIcons.Data.GetType()
                                                                .GetProperty("_" + summonerDetails.ProfileIconId)?.Name
                                                                ?.Substring(1) ?? "0", out var id)
                                                            ? id
                                                            : 0,
                                                    IsCaptain = player.UserId == team.CaptainUserId
                                                });
                                            }
                                            else
                                            {
                                                players.Add(new LeagueOfLegendsPlayer
                                                {
                                                    Name = player.InGameName.DisplayName,
                                                    Rank = "bronze_1",
                                                    NumericRank = 5,
                                                    PlayerId = summonerDetails.Id,
                                                    ProfileIconId =
                                                        int.TryParse(
                                                            profileIcons.Data.GetType()
                                                                .GetProperty("_" + summonerDetails.ProfileIconId)?.Name
                                                                ?.Substring(1) ?? "0", out var id)
                                                            ? id
                                                            : 0,
                                                    IsCaptain = player.UserId == team.CaptainUserId
                                                });
                                            }
                                        }
                                        catch (WebException ex)
                                        {
                                            if (ex.Status != WebExceptionStatus.ProtocolError || ex.Response == null)
                                                throw;

                                            var resp = (HttpWebResponse) ex.Response;
                                            if (resp.StatusCode == HttpStatusCode.ServiceUnavailable ||
                                                resp.StatusCode == HttpStatusCode.InternalServerError)
                                                goto retryTft;

                                            if (resp.StatusCode != HttpStatusCode.NotFound) throw;

                                            players.Add(new LeagueOfLegendsPlayer
                                            {
                                                Name = player.InGameName.DisplayName,
                                                NumericRank = 5,
                                                IsCaptain = player.UserId == team.CaptainUserId
                                            });
                                        }

                                        break;
                                    case ETournamentType.League:
                                        retryLoL:
                                        try
                                        {
                                            var data = web.DownloadString(
                                                $"https://euw1.api.riotgames.com/lol/summoner/v4/summoners/by-name/{player.InGameName.DisplayName}?api_key={config.RiotApiKey}");
                                            var summonerDetails = JsonConvert.DeserializeObject<Summoner>(data);
                                            var data2 = web.DownloadString(
                                                $"https://euw1.api.riotgames.com/lol/league/v4/entries/by-summoner/{summonerDetails.Id}?api_key={config.RiotApiKey}");
                                            var playerStats = JsonConvert.DeserializeObject<SummonerLeague[]>(data2);
                                            Thread.Sleep(2500);

                                            if ((playerStats?.Length ?? 0) > 0)
                                            {
                                                var soloRankedData = playerStats.FirstOrDefault(k =>
                                                    k.QueueType.Equals("RANKED_SOLO_5x5",
                                                        StringComparison.InvariantCultureIgnoreCase));
                                                players.Add(new LeagueOfLegendsPlayer
                                                {
                                                    Name = player.InGameName.DisplayName,
                                                    Rank = soloRankedData == null
                                                        ? "bronze_1"
                                                        : soloRankedData.Tier.ToLower() + "_" +
                                                          LeagueUtils.FromRomanToInt(soloRankedData.Rank),
                                                    NumericRank = soloRankedData == null
                                                        ? 5
                                                        : LeagueUtils.FromTierToInt(soloRankedData.Tier.ToLower()) +
                                                          LeagueUtils.FromRomanToInt(soloRankedData.Rank),
                                                    PlayerId = summonerDetails.Id,
                                                    ProfileIconId =
                                                        int.TryParse(
                                                            profileIcons.Data.GetType()
                                                                .GetProperty("_" + summonerDetails.ProfileIconId)?.Name
                                                                ?.Substring(1) ?? "0", out var id)
                                                            ? id
                                                            : 0,
                                                    IsCaptain = player.UserId == team.CaptainUserId
                                                });
                                            }
                                            else
                                            {
                                                players.Add(new LeagueOfLegendsPlayer
                                                {
                                                    Name = player.InGameName.DisplayName,
                                                    Rank = "bronze_1",
                                                    NumericRank = 5,
                                                    PlayerId = summonerDetails.Id,
                                                    ProfileIconId =
                                                        int.TryParse(
                                                            profileIcons.Data.GetType()
                                                                .GetProperty("_" + summonerDetails.ProfileIconId)?.Name
                                                                ?.Substring(1) ?? "0", out var id)
                                                            ? id
                                                            : 0,
                                                    IsCaptain = player.UserId == team.CaptainUserId
                                                });
                                            }
                                        }
                                        catch (WebException ex)
                                        {
                                            if (ex.Status != WebExceptionStatus.ProtocolError || ex.Response == null)
                                                throw;

                                            var resp = (HttpWebResponse) ex.Response;
                                            if (resp.StatusCode == HttpStatusCode.ServiceUnavailable ||
                                                resp.StatusCode == HttpStatusCode.InternalServerError)
                                                goto retryLoL;

                                            if (resp.StatusCode != HttpStatusCode.NotFound) throw;

                                            players.Add(new LeagueOfLegendsPlayer
                                            {
                                                Name = player.InGameName.DisplayName,
                                                NumericRank = 5,
                                                IsCaptain = player.UserId == team.CaptainUserId
                                            });
                                        }

                                        break;
                                }
                            }

                        var final = Teams.FirstOrDefault(k => k.Id.Equals(team.Id));
                        players = players.OrderBy(k => k.Name).ToList();

                        if (final == null)
                        {
                            TournamentTeam teamToAdd = null;

                            switch (tournament.TournamentType)
                            {
                                case ETournamentType.Rainbow6:
                                    teamToAdd = new RainbowSixTeam
                                    {
                                        Tournament = tournament,
                                        Id = team.Id,
                                        Name = team.Name,
                                        Members = players,
                                        AverageMmr = players.Sum(k =>
                                                         (k as RainbowSixPlayer).Mmr == 0
                                                             ? 2000
                                                             : (k as RainbowSixPlayer).Mmr) / players.Count
                                    };
                                    break;
                                case ETournamentType.TeamFightTactics:
                                case ETournamentType.League:
                                    teamToAdd = new LeagueOfLegendsTeam
                                    {
                                        Tournament = tournament,
                                        Id = team.Id,
                                        Name = team.Name,
                                        Members = players,
                                        AverageRank = LeagueUtils.FromIntToTierAndRank(
                                            players.Sum(k =>
                                                (k as LeagueOfLegendsPlayer).NumericRank == 0
                                                    ? 5
                                                    : (k as LeagueOfLegendsPlayer).NumericRank) / players.Count)
                                    };
                                    break;
                            }

                            if (teamToAdd == null) continue;

                            Teams.Add(teamToAdd);
                        }
                        else
                        {
                            final.Members = players;
                            switch (tournament.TournamentType)
                            {
                                case ETournamentType.Rainbow6:
                                    (final as RainbowSixTeam).AverageMmr =
                                        players.Sum(k =>
                                            (k as RainbowSixPlayer).Mmr == 0 ? 2000 : (k as RainbowSixPlayer).Mmr) /
                                        players.Count;
                                    break;
                                case ETournamentType.TeamFightTactics:
                                case ETournamentType.League:
                                    (final as LeagueOfLegendsTeam).AverageRank =
                                        LeagueUtils.FromIntToTierAndRank(
                                            players.Sum(k =>
                                                (k as LeagueOfLegendsPlayer).NumericRank == 0
                                                    ? 5
                                                    : (k as LeagueOfLegendsPlayer).NumericRank) / players.Count);
                                    break;
                            }
                        }
                    }
                }
            }

            Teams = Teams.OrderBy(k => k.Name).ToList();
        }

        public static IEnumerable<Team> GetNuelTeams(string nuelTapi, string nuelSapi, string tournamentName)
        {
            var teams = new List<Team>();
            using (var web = new WebClient())
            {
                var tournamentId = JsonConvert
                    .DeserializeObject<NuelTournament
                    >(web.DownloadString(nuelTapi + tournamentName)).Schedule
                    .FirstOrDefault(k => DateTime.UtcNow <= DateTime.Parse(k.Date).AddDays(1))?
                    .TournamentId;
                if (tournamentId != null)
                    teams.AddRange(JsonConvert
                        .DeserializeObject<TournamentSchedule>(web.DownloadString(nuelSapi + tournamentId)).Teams);
            }

            return teams;
        }
    }
}
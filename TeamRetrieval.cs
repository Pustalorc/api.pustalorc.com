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

                        foreach (var player in team.members.ToList())
                            if (player.inGameName?.displayName == null)
                            {
                                TeamPlayer playerToAdd = null;
                                switch (tournament.TournamentType)
                                {
                                    case ETournamentType.Rainbow6:
                                        playerToAdd = new RainbowSixPlayer
                                        {
                                            Name = player.userId, PlayerId = "",
                                            IsCaptain = player.userId == team.captainUserId
                                        };
                                        break;
                                    case ETournamentType.League:
                                        playerToAdd = new LeagueOfLegendsPlayer
                                        {
                                            Name = player.userId, NumericRank = 5,
                                            IsCaptain = player.userId == team.captainUserId
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
                                        }
                                        else
                                        {
                                            players.Add(new RainbowSixPlayer
                                            {
                                                Name = player.inGameName.displayName,
                                                PlayerId = "",
                                                IsCaptain = player.userId == team.captainUserId
                                            });
                                        }

                                        break;
                                    case ETournamentType.League:
                                        retry:
                                        try
                                        {
                                            var data = web.DownloadString(
                                                $"https://euw1.api.riotgames.com/lol/summoner/v4/summoners/by-name/{player.inGameName.displayName}?api_key={config.LoLApiKey}");
                                            var summonerDetails = JsonConvert.DeserializeObject<Summoner>(data);
                                            var data2 = web.DownloadString(
                                                $"https://euw1.api.riotgames.com/lol/league/v4/entries/by-summoner/{summonerDetails.id}?api_key={config.LoLApiKey}");
                                            var playerStats = JsonConvert.DeserializeObject<SummonerLeague[]>(data2);
                                            Thread.Sleep(2500);

                                            if ((playerStats?.Length ?? 0) > 0)
                                            {
                                                var soloRankedData = playerStats.FirstOrDefault(k =>
                                                    k.queueType.Equals("RANKED_SOLO_5x5",
                                                        StringComparison.InvariantCultureIgnoreCase));
                                                players.Add(new LeagueOfLegendsPlayer
                                                {
                                                    Name = player.inGameName.displayName,
                                                    Rank = soloRankedData == null
                                                        ? "bronze_1"
                                                        : soloRankedData.tier.ToLower() + "_" +
                                                          LeagueUtils.FromRomanToInt(soloRankedData.rank),
                                                    NumericRank =
                                                        LeagueUtils.FromTierToInt(soloRankedData.tier.ToLower()) +
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
                                            }
                                            else
                                            {
                                                players.Add(new LeagueOfLegendsPlayer
                                                {
                                                    Name = player.inGameName.displayName,
                                                    Rank = "bronze_1",
                                                    NumericRank = 5,
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
                                            }
                                        }
                                        catch (WebException ex)
                                        {
                                            if (ex.Status != WebExceptionStatus.ProtocolError || ex.Response == null)
                                                throw;

                                            var resp = (HttpWebResponse) ex.Response;
                                            if (resp.StatusCode == HttpStatusCode.ServiceUnavailable ||
                                                resp.StatusCode == HttpStatusCode.InternalServerError)
                                                goto retry;

                                            if (resp.StatusCode != HttpStatusCode.NotFound) throw;

                                            players.Add(new LeagueOfLegendsPlayer
                                            {
                                                Name = player.inGameName.displayName,
                                                NumericRank = 5,
                                                IsCaptain = player.userId == team.captainUserId
                                            });
                                        }

                                        break;
                                }
                            }

                        var final = Teams.FirstOrDefault(k => k.Id.Equals(team.id));
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
                                        Id = team.id,
                                        Name = team.name,
                                        Members = players,
                                        AverageMmr = players.Sum(k =>
                                                         (k as RainbowSixPlayer).Mmr == 0
                                                             ? 2000
                                                             : (k as RainbowSixPlayer).Mmr) / players.Count
                                    };
                                    break;
                                case ETournamentType.League:
                                    teamToAdd = new LeagueOfLegendsTeam
                                    {
                                        Tournament = tournament,
                                        Id = team.id,
                                        Name = team.name,
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
    }
}
using api.pustalorc.xyz.JSON_Classes;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using api.pustalorc.xyz.JSON_Classes.External_API.Nuel;
using api.pustalorc.xyz.JSON_Classes.External_API.R6S;

namespace api.pustalorc.xyz
{
    public static class RainbowSixTeams
    {
        public static List<RainbowSixTeam> Teams = new List<RainbowSixTeam>();

        public static void RetrieveGroups()
        {
            var configuration = ApiConfiguration.Load();
            var finalTeams = new List<RainbowSixTeam>();

            using (var web = new WebClient())
            {
                var teams = new List<Team>();

                foreach (var team in JsonConvert
                    .DeserializeObject<NuelTournament>(web.DownloadString(configuration.NuelTournamentApi + configuration.R6STournamentName))
                    .schedule.Where(k => k.isPlayableWeek && System.DateTime.UtcNow < System.DateTime.Parse(k.date)).ToList().ConvertAll(k => k.tournamentId).Select(id =>
                        JsonConvert.DeserializeObject<Tournament>(
                            web.DownloadString(configuration.NuelSignupPoolsApi + id)))
                    .Where(team => team.teams.Any()))
                    teams.AddRange(team.teams);

                foreach (var team in teams)
                {
                    var players = new List<RainbowSixPlayer>();
                    var teamMmr = 0;

                    foreach (var player in team.members.ToList())
                        if (player.inGameName?.displayName == null)
                        {
                            players.Add(new RainbowSixPlayer
                            {
                                Name = player.userId, Rank = 0, PlayerId = "", Mmr = 0,
                                IsCaptain = player.userId == team.captainUserId
                            });
                            teamMmr += 2000;
                        }
                        else
                        {
                            var downloadStr = "{\"totalresults\":0}";
                            try
                            {
                                downloadStr = web.DownloadString($"https://r6tab.com/api/search.php?platform=uplay&search={player.inGameName.displayName}");
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
                                    Name = player.inGameName.displayName, Rank = 0, PlayerId = "", Mmr = 0,
                                    IsCaptain = player.userId == team.captainUserId
                                });
                                teamMmr += 2000;
                            }
                        }

                    var final = finalTeams.FirstOrDefault(k => k.Id.Equals(team.id));
                    players = players.OrderBy(k => k.Name).ToList();

                    if (final == null)
                    {
                        finalTeams.Add(new RainbowSixTeam
                        {
                            Id = team.id, Name = team.name, Members = players,
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

            Teams = finalTeams.OrderBy(k => k.Name).ToList();
        }
    }
}
using api.pustalorc.xyz.JSON_Classes;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace api.pustalorc.xyz
{
    public static class RainbowSixTeams
    {
        public static List<SimpleTeam> Teams = new List<SimpleTeam>();

        public static void RetrieveGroups()
        {
            var finalTeams = new List<SimpleTeam>();
            using (var web = new WebClient())
            {
                var teams = new List<Team>();

                foreach (var team in JsonConvert
                    .DeserializeObject<NuelTournament>(web.DownloadString(
                        "https://tournament-cms.dev.thenuel.com/rainbow-six-siege-university-league-winter-2019"))
                    .schedule.ToList().ConvertAll(k => k.tournamentId).Select(id =>
                        JsonConvert.DeserializeObject<Tournament>(
                            web.DownloadString($"https://teams.dev.thenuel.com/signup-pools/{id}")))
                    .Where(team => team.teams.Any()))
                    teams.AddRange(team.teams.Where(k => k.members.Length >= 5).ToArray());

                foreach (var team in teams)
                {
                    var players = new List<SimplePlayer>();
                    var teamMMR = 0;

                    foreach (var player in team.members.ToList())
                    {
                        if (player.inGameName?.displayName == null)
                        {
                            players.Add(new SimplePlayer { Name = player.userId, Rank = 0, PlayerID = "", MMR = 0, IsCaptain = player.userId == team.captainUserId });
                        }
                        else
                        {
                            var playerData = JsonConvert.DeserializeObject<PlayerData>(
                                web.DownloadString($"https://r6tab.com/api/search.php?platform=uplay&search={player.inGameName.displayName}"));

                            if ((playerData?.results?.Length ?? 0) > 0)
                            {
                                var data = playerData.results[0];
                                players.Add(new SimplePlayer
                                {
                                    Name = player.inGameName?.displayName ?? player.userId,
                                    Rank = data.p_currentrank,
                                    PlayerID = data.p_user,
                                    MMR = data.p_currentmmr,
                                    IsCaptain = player.userId == team.captainUserId
                                });
                                teamMMR += data.p_currentmmr;
                            }
                            else
                                players.Add(new SimplePlayer { Name = player.inGameName?.displayName ?? player.userId, Rank = 0, PlayerID = "", MMR = 0, IsCaptain = player.userId == team.captainUserId });
                        }
                    }

                    var final = finalTeams.FirstOrDefault(k => k.Id.Equals(team.id));
                    var playersWithMMR = players.Count(k => k.MMR > 0);
                    players.OrderBy(k => k.IsCaptain);

                    if (final == null)
                    {
                        finalTeams.Add(new SimpleTeam
                        {
                            Id = team.id, Name = team.name, Members = players,
                            AverageMMR = playersWithMMR <= 0 ? 0 : teamMMR / playersWithMMR
                        });
                    }
                    else
                    {
                        final.Members = players;
                        final.AverageMMR = playersWithMMR <= 0 ? 0 : teamMMR / playersWithMMR;
                    }
                }
            }

            Teams = finalTeams.OrderBy(k => k.Name).ToList();
        }
    }
}
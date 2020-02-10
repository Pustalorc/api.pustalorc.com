using System;
using System.Collections.Generic;
using System.Linq;
using api.pustalorc.xyz.Configuration;
using api.pustalorc.xyz.JSON_Classes;
using Microsoft.AspNetCore.Mvc;

namespace api.pustalorc.xyz.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NuelTournamentsController : Controller
    {
        [HttpGet]
        public List<Tournament> Get()
        {
            return ApiConfiguration.Load().Tournaments;
        }

        [HttpGet("all")]
        public IEnumerable<TournamentTeam> GetAllTeams()
        {
            return TeamRetrieval.Teams;
        }

        [HttpGet("{tournament}")]
        public IEnumerable<TournamentTeam> GetTeamsInTournament(string tournament)
        {
            var config = ApiConfiguration.Load();
            var tournamentDetails = config.Tournaments.FirstOrDefault(k =>
                k.TournamentName.Equals(tournament, StringComparison.InvariantCultureIgnoreCase));

            return tournamentDetails.TournamentType switch
            {
                ETournamentType.TeamFightTactics => (IEnumerable<TournamentTeam>) TeamRetrieval.Teams
                    .Where(k => k.Tournament.TournamentName.Equals(tournament,
                        StringComparison.InvariantCultureIgnoreCase))
                    .ToList()
                    .ConvertAll(k => k as LeagueOfLegendsTeam),
                ETournamentType.League => TeamRetrieval.Teams
                    .Where(k => k.Tournament.TournamentName.Equals(tournament,
                        StringComparison.InvariantCultureIgnoreCase))
                    .ToList()
                    .ConvertAll(k => k as LeagueOfLegendsTeam),
                ETournamentType.Rainbow6 => TeamRetrieval.Teams
                    .Where(k => k.Tournament.TournamentName.Equals(tournament,
                        StringComparison.InvariantCultureIgnoreCase))
                    .ToList()
                    .ConvertAll(k => k as RainbowSixTeam),
                _ => new List<TournamentTeam>()
            };
        }

        [HttpGet("{tournament}/{team}")]
        public TournamentTeam GetTeamInTournament(string tournament, string team)
        {
            return TeamRetrieval.Teams
                .Where(k => k.Tournament.TournamentName.Equals(tournament, StringComparison.InvariantCultureIgnoreCase))
                .FirstOrDefault(k => k.Id == team);
        }
    }
}
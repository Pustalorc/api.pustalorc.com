using System.Collections.Generic;
using System.Linq;
using api.pustalorc.xyz.JSON_Classes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace api.pustalorc.xyz.Controllers
{
    [Route("nuelteams/[controller]")]
    [ApiController]
    public class LeagueOfLegendsController : ControllerBase
    {
        private readonly ILogger<LeagueOfLegendsController> _logger;

        public LeagueOfLegendsController(ILogger<LeagueOfLegendsController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<LeagueOfLegendsTeam> Get()
        {
            return LeagueOfLegendsTeams.Teams.ToArray();
        }

        [HttpGet("{team}")]
        public LeagueOfLegendsTeam GetTeam(string team)
        {
            return LeagueOfLegendsTeams.Teams.FirstOrDefault(k => k.Id == team);
        }
    }
}
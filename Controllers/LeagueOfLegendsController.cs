using System.Collections.Generic;
using System.Linq;
using api.pustalorc.xyz.JSON_Classes;
using Microsoft.AspNetCore.Mvc;

namespace api.pustalorc.xyz.Controllers
{
    [ApiController]
    [Route("NuelTeams/[controller]")]
    public class LeagueOfLegendsController : Controller
    {
        [HttpGet]
        public IEnumerable<LeagueOfLegendsTeam> Get()
        {
            return TeamRetrieval.LeagueTeams.ToArray();
        }

        [HttpGet("{team}")]
        public LeagueOfLegendsTeam GetTeam(string team)
        {
            return TeamRetrieval.LeagueTeams.FirstOrDefault(k => k.Id == team);
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using api.pustalorc.xyz.JSON_Classes;
using Microsoft.AspNetCore.Mvc;

namespace api.pustalorc.xyz.Controllers
{
    [ApiController]
    [Route("NuelTeams/[controller]")]
    public class RainbowSixController : Controller
    {
        [HttpGet]
        public IEnumerable<RainbowSixTeam> Get()
        {
            return TeamRetrieval.RainbowTeams.ToArray();
        }

        [HttpGet("{team}")]
        public RainbowSixTeam GetTeam(string team)
        {
            return TeamRetrieval.RainbowTeams.FirstOrDefault(k => k.Id == team);
        }
    }
}
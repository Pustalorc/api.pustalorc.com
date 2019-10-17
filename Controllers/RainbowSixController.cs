using System.Collections.Generic;
using System.Linq;
using api.pustalorc.xyz.JSON_Classes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace api.pustalorc.xyz.Controllers
{
    [ApiController]
    [Route("nuelteams/[controller]")]
    public class RainbowSixController : ControllerBase
    {
        private readonly ILogger<RainbowSixController> _logger;

        public RainbowSixController(ILogger<RainbowSixController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<SimpleTeam> Get()
        {
            return RainbowSixTeams.Teams.ToArray();
        }

        [HttpGet("{team}")]
        public SimpleTeam GetTeam(string team)
        {
            return RainbowSixTeams.Teams.FirstOrDefault(k => k.Id == team);
        }
    }
}
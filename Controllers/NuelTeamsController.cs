using System.Collections.Generic;
using System.Linq;
using System.Net;
using api.pustalorc.xyz.JSON_Classes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace api.pustalorc.xyz.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NuelTeamsController : ControllerBase
    {
        private readonly ILogger<NuelTeamsController> _logger;

        public NuelTeamsController(ILogger<NuelTeamsController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<SimpleTeam> Get()
        {
            return RemoteData._simpleTeams.ToArray();
        }
    }
}
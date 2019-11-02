using System.Collections.Generic;
using api.pustalorc.xyz.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace api.pustalorc.xyz.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NuelTeamsController : Controller
    {
        [HttpGet]
        public List<Tournament> Get()
        {
            return ApiConfiguration.Load().Tournaments;
        }
    }
}
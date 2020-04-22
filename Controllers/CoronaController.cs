using BeDudeApi.Models;
using BeDudeApi.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace BeDudeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoronaController : ControllerBase
    {
        private readonly CoronaStatusService _coronaStatusService;


        public CoronaController(CoronaStatusService coronaStatusService)
        {
            _coronaStatusService = coronaStatusService;
        }

        [HttpGet]
        public ActionResult<List<CoronaStatus>> Get()
        {
            return _coronaStatusService.Get();
        }
    }
}
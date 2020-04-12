using BeDudeApi.Models;
using BeDudeApi.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace BeDudeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoronaStatusController : ControllerBase
    {
        private readonly CoronaStatusService _coronaStatusService;


        public CoronaStatusController(CoronaStatusService coronaStatusService)
        {
            _coronaStatusService = coronaStatusService;
        }

        [HttpGet]
        public ActionResult<List<CoronaSatausResponse>> Get() => _coronaStatusService.GetCoronaChartData();

        [HttpGet("{id:length(24)}", Name = "GetCoronaStatus")]
        public ActionResult<CoronaStatus> Get(string id)
        {
            var coronaStatus = _coronaStatusService.Get(id);

            if (coronaStatus == null)
            {
                return NotFound();
            }

            return coronaStatus;
        }

        [HttpPost]
        public ActionResult<CoronaStatus> Create(CoronaStatus coronaStatus)
        {
            _coronaStatusService.Create(coronaStatus);

            return CreatedAtRoute("GetCoronaStatus", new { id = coronaStatus.Id.ToString() }, coronaStatus);
        }

    }
}
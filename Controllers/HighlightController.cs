using auto_highlighter_back_end.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace auto_highlighter_back_end.Controllers
{
    [ApiController]
    [Route("/api-v1/[controller]")]
    public class HighlightController : ControllerBase
    {
        private readonly ILogger<HighlightController> _logger;

        [HttpGet("{hid}")]
        public IActionResult Get(Guid hid)
        {

            //get db stuff here instead of random numbers:)

            Random rnd = new Random();
            HighlightStatusResponse response = new HighlightStatusResponse
            {
                hid = Guid.NewGuid(),
                Status = HighlightStatusEnum.Ready.ToString()
            };

            return new OkObjectResult(response);
        }

        [HttpPost]
        public IActionResult Post()
        {

            //get db stuff here instead of random numbers:)
            HighlightStatusResponse response = new HighlightStatusResponse
            {
                hid = Guid.NewGuid(),
                Status = HighlightStatusEnum.Processing.ToString()
            };

            return StatusCode(201, response);
        }

    }
}

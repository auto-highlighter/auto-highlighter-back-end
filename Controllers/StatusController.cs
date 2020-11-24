using auto_highlighter_back_end.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace auto_highlighter_back_end.Controllers
{
    [Route("/api-v1/[controller]")]
    [ApiController]
    public class StatusController : ControllerBase
    {

        private readonly ILogger<StatusController> _logger;

        public StatusController(ILogger<StatusController> logger)
        {
            _logger = logger;
        }

        [HttpGet("{hid}")]
        public IActionResult Get(Guid hid)
        {

            //get db stuff here instead of random numbers:)

            Random rnd = new Random();
            HighlightStatusResponse response = new HighlightStatusResponse
            {
                hid = hid
            };

            if (rnd.Next(1, 3) == 1)
            {
                response.Status = HighlightStatusEnum.Processing.ToString();
            }
            else
            {
                response.Status = HighlightStatusEnum.Ready.ToString();
            }

            return new OkObjectResult(response);
        }
    }
}

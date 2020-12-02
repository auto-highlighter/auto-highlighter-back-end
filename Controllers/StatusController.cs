using auto_highlighter_back_end.Enums;
using auto_highlighter_back_end.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using auto_highlighter_back_end.Repository;
using auto_highlighter_back_end.Extentions;
using auto_highlighter_back_end.Entity;

namespace auto_highlighter_back_end.Controllers
{
    [Route("/api-v1/[controller]")]
    [ApiController]
    public class StatusController : ControllerBase
    {


        private readonly ILogger _logger;
        private readonly ITempHighlightRepo _repository;

        public StatusController(ITempHighlightRepo repository, ILogger<StatusController> logger)
        {
            _logger = logger;
            _repository = repository;
        }

        [HttpGet("{hid}")]
        public IActionResult GetHighlightStatus(Guid hid)
        {

            //get db stuff here instead of random numbers:)
            HighlightEntity highlight = _repository.GetHighlight(hid);

            if(highlight is null)
            {
                return NotFound();
            }

            HighlightStatusDTO response = highlight.AsDto();

            return Ok(response);
        }
    }
}

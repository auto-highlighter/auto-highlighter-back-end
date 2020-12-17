using auto_highlighter_back_end.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using auto_highlighter_back_end.Repository;
using auto_highlighter_back_end.Extentions;
using auto_highlighter_back_end.Entity;
using auto_highlighter_back_end.Attributes;
using System.Collections.Generic;
using System.Linq;

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

        [HttpGet]
        [RateLimit(1000)]
        public IActionResult GetHighlights()
        {

            IEnumerable<HighlightStatusDTO> response = from highlightEntity in _repository.GetHighlights() select highlightEntity.AsDto();

            return Ok(response);
        }

        [HttpGet("{hid}")]
        [RateLimit(1000)]
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

using auto_highlighter_back_end.Enums;
using auto_highlighter_back_end.DTOs;
using auto_highlighter_back_end.Extentions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using auto_highlighter_back_end.Repository;
using auto_highlighter_back_end.Entity;
using System.IO.Compression;

namespace auto_highlighter_back_end.Controllers
{
    [ApiController]
    [Route("/api-v1/[controller]")]
    public class HighlightController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly ITempHighlightRepo _repository;

        public HighlightController(ITempHighlightRepo repository, ILogger<HighlightController>   logger)
        {
            _logger = logger;
            _repository = repository;
        }

        [HttpGet]
        public IActionResult GetHighlights()
        {

            //get db stuff here instead of random numbers:)

            IEnumerable<HighlightStatusDTO> response = from highlightEntity in _repository.GetHighlights() select highlightEntity.AsDto();

            return Ok(response);
        }

        [HttpGet("{hid}")]
        public IActionResult DownloadHighlight(Guid hid)
        {

            //get db stuff here instead of random numbers:)
            HighlightStatusDTO response = _repository.GetHighlight(hid).AsDto();

            if (response is null)
            {
                return NotFound();
            }

            return File(Array.Empty<byte>(), "application/zip");
        }

        [HttpPost]
        public IActionResult CreateHighlight()
        {

            //get db stuff here instead of random numbers:)
            HighlightEntity highlightEntity = new()
            {
                Hid = Guid.NewGuid(),
                Status = HighlightStatusEnum.Processing.ToString(),
                CreatedTimestamp = DateTimeOffset.UtcNow
            };

            _repository.CreateHighlight(highlightEntity);

            return CreatedAtAction(nameof(CreateHighlight), new { id = highlightEntity.Hid }, highlightEntity.AsDto());
        }

    }
}

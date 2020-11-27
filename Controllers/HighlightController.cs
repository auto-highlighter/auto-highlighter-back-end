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
using System.IO;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using System.Text;

namespace auto_highlighter_back_end.Controllers
{
    [ApiController]
    [Route("/api-v1/[controller]")]
    public class HighlightController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly ITempHighlightRepo _repository;
        private readonly IConfiguration _config;

        public HighlightController(ITempHighlightRepo repository, ILogger<HighlightController> logger, IConfiguration config)
        {
            _logger = logger;
            _repository = repository;
            _config = config;
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

        [HttpPost("[controller]/[action]")]
        [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            _logger.LogInformation($"Request for file upload of {file.Length} Bytes ({file.Length / Math.Pow(10, 9)} GB)");

            if (file is null)
            {
                return BadRequest();
            }

            string fileuploadPath = _config.GetValue<string>("FileUploadLocation");

            using StreamReader sr = new(file.OpenReadStream());

            string content = await sr.ReadToEndAsync();

            _logger.LogInformation($"Content size is {System.Text.Encoding.ASCII.GetByteCount(content)}");

            GC.Collect();

            return CreatedAtAction(nameof(CreateHighlight), new { message = "Success" });

        }

        [HttpPost("[controller]/[action]")]
        public IActionResult StartUpload(IFormFile file)
        {
            return StatusCode(501, new NotImplementedException());

        }

        [HttpPost("[controller]/[action]")]
        public IActionResult UploadChunk(IFormFile file)
        {
            return StatusCode(501, new NotImplementedException());

        }

        [HttpPost("[controller]/[action]")]
        public IActionResult FinishUpload(IFormFile file)
        {
            return StatusCode(501, new NotImplementedException());

        }
    }
}

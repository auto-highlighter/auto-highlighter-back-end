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
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using auto_highlighter_back_end.Services;

namespace auto_highlighter_back_end.Controllers
{
    [ApiController]
    [Route("/api-v1/[controller]")]
    public class HighlightController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly ITempHighlightRepo _repository;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;
        //private readonly IBlobService _blobService;
        private readonly IVideoProcessService _videoProcessService;

        public HighlightController(ITempHighlightRepo repository, ILogger<HighlightController> logger, IConfiguration config, IWebHostEnvironment env, IVideoProcessService videoProcessService)
        {
            _logger = logger;
            _repository = repository;
            _config = config;
            _env = env;
            //_blobService = blobService;
            _videoProcessService = videoProcessService;

        }

        [HttpGet]
        public IActionResult GetHighlights()
        {

            //get db stuff here instead of random numbers:)

            IEnumerable<HighlightStatusDTO> response = from highlightEntity in _repository.GetHighlights() select highlightEntity.AsDto();

            return Ok(response);
        }

        [HttpGet("{hid}")]
        public async Task<IActionResult> DownloadHighlight(Guid hid)
        {

            //get db stuff here instead of random numbers:)
            HighlightEntity response = _repository.GetHighlight(hid);

            if (response is null)
            {
                return NotFound();
            }

            if (response.Status != HighlightStatusEnum.Done.ToString())
            {
                return Accepted();
            }

            string filePath = Path.Combine(
                _env.ContentRootPath,
                _config.GetValue<string>("FileUploadLocation"),
                response.Hid.ToString());

            byte[] file = await System.IO.File.ReadAllBytesAsync(filePath);

            System.IO.File.Delete(filePath);

            return File(file, "video/mp4");
        }

        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> CreateHighlight(IFormFile file)
        {

            if (file.Length <= 0)
            {
                return BadRequest();
            }

            Guid hid = Guid.NewGuid();


            string filePath = Path.Combine(_env.ContentRootPath, _config.GetValue<string>("FileUploadLocation"));

            Directory.CreateDirectory(filePath);

            filePath = Path.Combine(filePath, hid.ToString());

            using Stream fileStream = new FileStream(filePath, FileMode.Create);

            await file.CopyToAsync(fileStream);

            HighlightEntity highlightEntity = new()
            {
                Hid = hid,
                Status = HighlightStatusEnum.Ready.ToString(),
                CreatedTimestamp = DateTimeOffset.UtcNow
            };

            _repository.CreateHighlight(highlightEntity);

            return CreatedAtAction(nameof(CreateHighlight), new { id = highlightEntity.Hid }, highlightEntity.AsDto());
        }

        [HttpPut("{hid}")]
        public async Task<IActionResult> ProccessHighlight(Guid hid)
        {
            await _videoProcessService.ProcessHightlightAsync(hid);
            return Ok();
        }

        [HttpPost("[action]")]
        [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
        [DisableRequestSizeLimit]
        public IActionResult UploadToBlob(IFormFile file)
        {
            return StatusCode(StatusCodes.Status501NotImplemented);
            /* Leaving all this commented for now for MVP 
            if (file is null)
            {
                return BadRequest();
            }

            Uri result = await _blobService.UploadFileBlobAsync(
                    "firstcontainer",
                    file.OpenReadStream(),
                    file.ContentType,
                    file.FileName);

            return CreatedAtAction(nameof(CreateHighlight), result.AbsoluteUri);*/
        }
    }
}

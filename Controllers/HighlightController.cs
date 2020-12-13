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
using auto_highlighter_back_end.Attributes;

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
        [RateLimit(1000)]
        public IActionResult GetHighlights()
        {

            //get db stuff here instead of random numbers:)

            IEnumerable<HighlightStatusDTO> response = from highlightEntity in _repository.GetHighlights() select highlightEntity.AsDto();

            return Ok(response);
        }

        [HttpGet("{hid}")]
        [RateLimit(10000)]
        public async Task<IActionResult> DownloadHighlight(Guid hid)
        {

            //get db stuff here instead of random numbers:)
            HighlightEntity highlight = _repository.GetHighlight(hid);

            if (highlight is null)
            {
                return NotFound();
            }

            if (highlight.Status != HighlightStatusEnum.Done.ToString())
            {
                return Accepted(highlight);
            }


            string filePath = Path.Combine(_env.ContentRootPath, _config.GetValue<string>("FileUploadLocation"), hid.ToString());

            string vodFilePath = filePath + ".mp4";
            string timeStampsFilePath = filePath + ".json";

            byte[] file = await System.IO.File.ReadAllBytesAsync(vodFilePath);

            System.IO.File.Delete(vodFilePath);
            System.IO.File.Delete(timeStampsFilePath);
            _repository.RemoveHighlight(hid);

            return File(file, "video/mp4");
        }

        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
        [DisableRequestSizeLimit]
        [RateLimit(10000)]
        public async Task<IActionResult> CreateHighlight(IFormFile vod, IFormFile timeStamps)
        {

            if (vod is null || timeStamps is null || vod.Length <= 0 || timeStamps.Length <= 0)
            {
                return BadRequest();
            }

            Guid hid = Guid.NewGuid();


            string filePath = Path.Combine(_env.ContentRootPath, _config.GetValue<string>("FileUploadLocation"));

            Directory.CreateDirectory(filePath);

            string vodFilePath = Path.Combine(filePath, hid.ToString() + Path.GetExtension(vod.FileName));
            string timeStampsFilePath = Path.Combine(filePath, hid.ToString() + Path.GetExtension(timeStamps.FileName));

            using Stream vodFileStream = new FileStream(vodFilePath, FileMode.Create);
            using Stream timeStampsFileStream = new FileStream(timeStampsFilePath, FileMode.Create);

            _ = timeStamps.CopyToAsync(timeStampsFileStream);
            await vod.CopyToAsync(vodFileStream);


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
        [RateLimit(1000)]
        public async Task<IActionResult> ProccessHighlight(Guid hid)
        {

            HighlightEntity highlight = _repository.GetHighlight(hid);
            if (highlight is null)
            {
                return NotFound();
            }

            await _videoProcessService.ProcessHightlightAsync(highlight);
            return NoContent();
        }

        [HttpPost("[action]")]
        [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
        [DisableRequestSizeLimit]
        [RateLimit(10000)]
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

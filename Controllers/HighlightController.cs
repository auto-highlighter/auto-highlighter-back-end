﻿using auto_highlighter_back_end.Enums;
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
using System.Net;
using Microsoft.AspNetCore.Hosting;
using auto_highlighter_back_end.Services;
using auto_highlighter_back_end.Attributes;
using Utf8Json;
using Azure.Storage.Blobs.Models;

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
        private readonly IBlobService _blobService;
        private readonly IVideoProcessService _videoProcessService;
        private readonly IMessageQueueService _messageQueue;

        public HighlightController(ITempHighlightRepo repository, ILogger<HighlightController> logger, IConfiguration config, IWebHostEnvironment env, IBlobService blobService, IVideoProcessService videoProcessService, IMessageQueueService messageQueue)
        {
            _logger = logger;
            _repository = repository;
            _config = config;
            _env = env;
            _blobService = blobService;
            _videoProcessService = videoProcessService;
            _messageQueue = messageQueue;
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

            if (highlight.Status != HighlightStatusEnum.Done.ToString())
            {
                return Accepted(highlight);
            }

            BlobDownloadInfo vod = await _blobService.GetBlobAsync(_config["BlobSettings:ContainerName"], hid.ToString() + ".mp4");

            Task[] deletions = new Task[2];
            deletions[0] = _blobService.DeleteBlobAsync(_config["BlobSettings:ContainerName"], hid.ToString() + ".mp4");
            deletions[1] = _blobService.DeleteBlobAsync(_config["BlobSettings:ContainerName"], hid.ToString() + ".json");

            Task.WaitAll(deletions);

            _repository.RemoveHighlight(hid);

            return File(vod.Content, vod.ContentType);
        }

        [HttpPost]
        public IActionResult CreateHighlight()
        {

            if (vod is null || timeStamps is null || vod.Length <= 0 || timeStamps.Length <= 0)
            {
                return BadRequest();
            }

            Guid hid = Guid.NewGuid();


            string filePath = Path.Combine(_env.ContentRootPath, _config.GetValue<string>("FileUploadLocation"));

            Directory.CreateDirectory(filePath);

            /*string vodFilePath = Path.Combine(filePath, hid.ToString() + Path.GetExtension(vod.FileName));
            string timeStampsFilePath = Path.Combine(filePath, hid.ToString() + Path.GetExtension(timeStamps.FileName));

            using Stream vodFileStream = new FileStream(vodFilePath, FileMode.Create);
            using Stream timeStampsFileStream = new FileStream(timeStampsFilePath, FileMode.Create);

            _ = timeStamps.CopyToAsync(timeStampsFileStream);
            await vod.CopyToAsync(vodFileStream);*/

            Task[] uploads = new Task[2];

            uploads[0] = _blobService.UploadFileBlobAsync(
                    _config["BlobSettings:ContainerName"],
                    vod.OpenReadStream(),
                    vod.ContentType,
                    hid + ".mp4");

            uploads[1] = _blobService.UploadFileBlobAsync(
                    _config["BlobSettings:ContainerName"],
                    timeStamps.OpenReadStream(),
                    timeStamps.ContentType,
                    hid + ".json");

            await Task.WhenAll(uploads);


            HighlightEntity highlightEntity = new()
            {
                Hid = Guid.NewGuid(),
                Status = HighlightStatusEnum.Processing.ToString(),
                CreatedTimestamp = DateTimeOffset.UtcNow
            };

            _repository.CreateHighlight(highlightEntity);

            return CreatedAtAction(nameof(CreateHighlight), new { id = highlightEntity.Hid }, highlightEntity.AsDto());
        }

        [HttpPost("[action]")]
        [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file is null)
            {
                return BadRequest();
            }

            Uri result = await _blobService.UploadFileBlobAsync(
                    "firstcontainer",
                    file.OpenReadStream(),
                    file.ContentType,
                    file.FileName);

            return CreatedAtAction(nameof(CreateHighlight), result.AbsoluteUri);
        }

        [HttpPost("[action]")]
        [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
        [DisableRequestSizeLimit]
        [RateLimit(10000)]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file is null)
            {
                return BadRequest();
            }

            Uri result = await _blobService.UploadFileBlobAsync(
                    _config["BlobSettings:ContainerName"],
                    file.OpenReadStream(),
                    file.ContentType,
                    file.FileName);

            return CreatedAtAction(nameof(CreateHighlight), result.AbsoluteUri);
        }
    }
}

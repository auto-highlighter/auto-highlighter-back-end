using auto_highlighter_back_end.Entity;
using auto_highlighter_back_end.Enums;
using auto_highlighter_back_end.Repository;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Utf8Json;

namespace auto_highlighter_back_end.Services
{
    public class VideoProcessService : IVideoProcessService
    {
        private readonly ILogger _logger;
        private readonly ITempHighlightRepo _repository;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;

        public VideoProcessService(ILogger<IVideoProcessService> logger, ITempHighlightRepo repository, IConfiguration config, IWebHostEnvironment env)
        {
            _logger = logger;
            _repository = repository;
            _config = config;
            _env = env;
        }
        public async Task ProcessHightlightAsync(HighlightEntity highlight)
        {
            _logger.LogInformation($"Processing video {highlight.Hid}");

            highlight = new()
            {
                Hid = highlight.Hid,
                Status = HighlightStatusEnum.Processing.ToString(),
                CreatedTimestamp = highlight.CreatedTimestamp
            };

            _repository.UpdateHighlight(highlight);

            byte[] vod = GetVod(highlight.Hid);
            List<int> timestamps = GetTimestamps(highlight.Hid);

            await Task.Delay(1000); //simulate processing
            highlight = new()
            {
                Hid = highlight.Hid,
                Status = HighlightStatusEnum.Done.ToString(),
                CreatedTimestamp = highlight.CreatedTimestamp
            };

            _repository.UpdateHighlight(highlight);
            _logger.LogInformation($"Finished video {highlight.Hid}");
        }

        private byte[] GetVod(Guid hid)
        {
            string filePath = Path.Combine(_env.ContentRootPath, _config.GetValue<string>("FileUploadLocation"), hid.ToString());

            string vodFilePath = filePath + ".mp4";

            return File.ReadAllBytes(vodFilePath);
        }

        private List<int> GetTimestamps(Guid hid)
        {
            string filePath = Path.Combine(_env.ContentRootPath, _config.GetValue<string>("FileUploadLocation"), hid.ToString());

            string timeStampsFilePath = filePath + ".json";
            byte[] file = File.ReadAllBytes(timeStampsFilePath);
            _logger.LogInformation(Encoding.UTF8.GetString(file));
            List<int> timestamps = JsonSerializer.Deserialize<List<int>>(file);
            return timestamps;
        }
    }
}

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
using Xabe.FFmpeg;

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

            await EditVideo(highlight.Hid, timestamps);

            //await Task.Delay(1000); //simulate processing

            highlight = new()
            {
                Hid = highlight.Hid,
                Status = HighlightStatusEnum.Done.ToString(),
                CreatedTimestamp = highlight.CreatedTimestamp
            };
            _repository.UpdateHighlight(highlight);

            _logger.LogInformation($"Finished video {highlight.Hid}");
        }

        private async Task EditVideo(Guid hid, List<int> timestamps)
        {
            string vodFilePath = Path.Combine(_env.ContentRootPath, _config.GetValue<string>("FileUploadLocation"), hid.ToString());

            IConversion conversion;

            int startTime;
            int endTime;
            string[] fileNames = new string[timestamps.Count];

            Task[] tasks = new Task[timestamps.Count];
            for (int index = 0; index < timestamps.Count; index++)
            {
                endTime = timestamps[index];
                startTime = endTime < 30000 ? 0 : endTime - 30000;
                fileNames[index] = vodFilePath + index.ToString() + ".mp4";

                _logger.LogInformation($"Start Time: {startTime} | End Time: {endTime} | File Name: {fileNames[index]}");

                conversion = await FFmpeg.Conversions.FromSnippet.Split(vodFilePath + ".mp4", fileNames[index], TimeSpan.FromMilliseconds(startTime), TimeSpan.FromMilliseconds(30000));

                tasks[index] = conversion.Start();
            }

            await Task.WhenAll(tasks);


            if (timestamps.Count > 1)
            {
                File.Delete(vodFilePath + ".mp4");
                conversion = await FFmpeg.Conversions.FromSnippet.Concatenate(vodFilePath + ".mp4", fileNames);
                await conversion.Start();
            }

            foreach (string fileName in fileNames)
            {
                File.Delete(fileName);
            }


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

            List<int> timestamps = JsonSerializer.Deserialize<List<int>>(file);
            return timestamps;
        }
    }
}

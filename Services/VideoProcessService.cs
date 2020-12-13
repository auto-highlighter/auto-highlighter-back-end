using auto_highlighter_back_end.Entity;
using auto_highlighter_back_end.Enums;
using auto_highlighter_back_end.Repository;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
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
            
            List<int> timestamps = GetTimestamps(highlight.Hid);
            List<HighlightTimeSpan> highlightTimeSpans = ToHighlightTimeSpans(timestamps);

            await EditVideo(highlight.Hid, highlightTimeSpans);

            highlight = new()
            {
                Hid = highlight.Hid,
                Status = HighlightStatusEnum.Done.ToString(),
                CreatedTimestamp = highlight.CreatedTimestamp
            };
            _repository.UpdateHighlight(highlight);

            _logger.LogInformation($"Finished video {highlight.Hid}");
        }
        
        private async Task EditVideo(Guid hid, List<HighlightTimeSpan> highlightTimeSpans)
        {
            string vodFilePath = Path.Combine(_env.ContentRootPath, _config.GetValue<string>("FileUploadLocation"), hid.ToString());

            IConversion conversion;

            int start;
            int duration;
            string[] fileNames = new string[highlightTimeSpans.Count];

            Task[] tasks = new Task[highlightTimeSpans.Count];
            for (int index = 0; index < highlightTimeSpans.Count; index++)
            {
                fileNames[index] = vodFilePath + index.ToString() + ".mp4";
                start = highlightTimeSpans[index].Start;
                duration = highlightTimeSpans[index].Duration;

                _logger.LogInformation($"Start Time: {start} | End Time: {start + duration} | File Name: {fileNames[index]}");

                conversion = await FFmpeg.Conversions.FromSnippet.Split(vodFilePath + ".mp4", fileNames[index], TimeSpan.FromMilliseconds(start), TimeSpan.FromMilliseconds(duration));

                tasks[index] = conversion.Start();
            }

            await Task.WhenAll(tasks);

            if (highlightTimeSpans.Count > 1)
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

        private List<HighlightTimeSpan> ToHighlightTimeSpans(List<int> timestamps)
        {
            List<HighlightTimeSpan> highlightTimeSpans = new();
            int highlightLength = int.Parse(_config["HighlightSettings:HighlightLength"]);
            int startTime = -1;
            for (int index = 0; index < timestamps.Count; index++)
            {
                if (startTime == -1)
                {
                    startTime = timestamps[index] - highlightLength;

                    if (startTime < 0)
                    {
                        startTime = 0;
                    }
                }

                if (index == timestamps.Count - 1 || timestamps[index + 1] - highlightLength > timestamps[index])
                {
                    highlightTimeSpans.Add(new(startTime, timestamps[index] - startTime));
                    startTime = -1;
                }
            }
            return highlightTimeSpans;
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

    public struct HighlightTimeSpan
    {
        public HighlightTimeSpan(int start, int duration)
        {
            Start = start;
            Duration = duration;
        }

        public int Start { get; }
        public int Duration { get; }
    }
}

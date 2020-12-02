using auto_highlighter_back_end.Entity;
using auto_highlighter_back_end.Enums;
using auto_highlighter_back_end.Repository;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace auto_highlighter_back_end.Services
{
    public class VideoProcessService : IVideoProcessService
    {
        private readonly ILogger _logger;
        private readonly ITempHighlightRepo _repository;

        public VideoProcessService(ILogger<IVideoProcessService> logger, ITempHighlightRepo repository)
        {
            _logger = logger;
            _repository = repository;
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
    }
}

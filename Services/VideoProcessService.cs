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
        public async Task ProcessHightlightAsync(Guid hid)
        {
            _logger.LogInformation($"Processing video {hid}");

            HighlightEntity highlightEntity = _repository.GetHighlight(hid);
            highlightEntity = new()
            {
                Hid = highlightEntity.Hid,
                Status = HighlightStatusEnum.Processing.ToString(),
                CreatedTimestamp = highlightEntity.CreatedTimestamp
            };

            _repository.UpdateHighlight(highlightEntity);

            await Task.Delay(10000); //simulate processing
            highlightEntity = new()
            {
                Hid = highlightEntity.Hid,
                Status = HighlightStatusEnum.Done.ToString(),
                CreatedTimestamp = highlightEntity.CreatedTimestamp
            };

            _repository.UpdateHighlight(highlightEntity);
            _logger.LogInformation($"Finished video {hid}");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using auto_highlighter_back_end.DTOs;
using auto_highlighter_back_end.Entity;
using auto_highlighter_back_end.Enums;
using auto_highlighter_back_end.Repository;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Utf8Json;

namespace auto_highlighter_back_end.Services
{
    public class MessageQueueService : IMessageQueueService
    {
        private readonly ILogger _logger;
        private readonly ServiceBusClient _serviceBusClient;
        private readonly IConfiguration _config;
        private readonly IVideoProcessService _videoProcessService;
        private readonly ITempHighlightRepo _repository;

        public MessageQueueService(ILogger<IMessageQueueService> logger, ServiceBusClient serviceBusClient, IConfiguration config, IVideoProcessService videoProcessService, ITempHighlightRepo repository)
        {
            _logger = logger;
            _serviceBusClient = serviceBusClient;
            _config = config;
            _videoProcessService = videoProcessService;
            _repository = repository;
        }
        public async Task SendMessageAsync(byte[] messageBody)
        {
            _logger.LogInformation("Started sending message");

            ServiceBusSender sender = _serviceBusClient.CreateSender(_config["ServiceBus:QueueName"]);

            ServiceBusMessage message = new ServiceBusMessage(messageBody);

            try
            {
                await sender.SendMessageAsync(message);
                _logger.LogInformation("Message sent");
            }
            catch (Exception e)
            {
                _logger.LogInformation($"Failed to send {e.Message}");
            }
        }


        public async Task ReceiveMessagesAsync()
        {
            ServiceBusProcessor processor = _serviceBusClient.CreateProcessor(_config["ServiceBus:QueueName"], new ServiceBusProcessorOptions());

            processor.ProcessMessageAsync += MessageHandler;
            processor.ProcessErrorAsync += ErrorHandler;

            await processor.StartProcessingAsync();
        }

        // handle received messages
        private async Task MessageHandler(ProcessMessageEventArgs args)
        {
            string body = args.Message.Body.ToString();


            _logger.LogInformation($"Recieved message {body}");
            try
            {

                ProccessVodDTO proccessVodDTO = JsonSerializer.Deserialize<ProccessVodDTO>(body);
                HighlightEntity highlight = _repository.GetHighlight(proccessVodDTO.Hid);

                if (highlight is not null)
                {
                    Task processVod = _videoProcessService.ProcessHightlightAsync(highlight);

                    highlight = new()
                    {
                        Hid = highlight.Hid,
                        Status = HighlightStatusEnum.Processing.ToString(),
                        CreatedTimestamp = highlight.CreatedTimestamp
                    };
                    _repository.UpdateHighlight(highlight);

                    processVod.Wait();

                    highlight = new()
                    {
                        Hid = highlight.Hid,
                        Status = HighlightStatusEnum.Done.ToString(),
                        CreatedTimestamp = highlight.CreatedTimestamp
                    };
                    _repository.UpdateHighlight(highlight);
                }
            }
            catch (Exception e)
            {
                _logger.LogInformation($"caught exception in message {body} processing: {e.Message}");
            }

            await args.CompleteMessageAsync(args.Message);
        }

        // handle any errors when receiving messages
        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }
    }
}
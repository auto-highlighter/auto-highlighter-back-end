using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using auto_highlighter_back_end.DTOs;
using auto_highlighter_back_end.Entity;
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
            // create a sender for the queue 
            ServiceBusSender sender = _serviceBusClient.CreateSender(_config["ServiceBus:Name"]);

            // create a message that we can send
            ServiceBusMessage message = new ServiceBusMessage(messageBody);

            // send the message
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
            // create a processor that we can use to process the messages
            ServiceBusProcessor processor = _serviceBusClient.CreateProcessor(_config["ServiceBus:Name"], new ServiceBusProcessorOptions());

            // add handler to process messages
            processor.ProcessMessageAsync += MessageHandler;

            // add handler to process any errors
            processor.ProcessErrorAsync += ErrorHandler;

            // start processing 
            await processor.StartProcessingAsync();
        }

        // handle received messages
        private async Task MessageHandler(ProcessMessageEventArgs args)
        {
            string body = args.Message.Body.ToString();


            _logger.LogInformation($"Recieved message {body}");

            ProccessVodDTO proccessVodDTO = JsonSerializer.Deserialize<ProccessVodDTO>(body);
            HighlightEntity highlight = _repository.GetHighlight(proccessVodDTO.Hid);

            if (highlight is not null)
            {
                _ = _videoProcessService.ProcessHightlightAsync(highlight);
            }

            // complete the message. messages is deleted from the queue. 
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
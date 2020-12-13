using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;

namespace auto_highlighter_back_end.Services
{
    public class MessageQueueService : IMessageQueueService
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly IConfiguration _config;

        public MessageQueueService(ServiceBusClient serviceBusClient, IConfiguration config)
        {
            _serviceBusClient = serviceBusClient;
            _config = config;
        }
        public async Task SendMessageAsync(string messageBody)
        {
            // create a sender for the queue 
            ServiceBusSender sender = _serviceBusClient.CreateSender(_config["ServiceBus:Name"]);

            // create a message that we can send
            ServiceBusMessage message = new ServiceBusMessage(messageBody);

            // send the message
            await sender.SendMessageAsync(message);
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
            Console.WriteLine($"Received: {body}");

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
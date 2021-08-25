using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBotExtensions.Interfaces;
using static System.Threading.Tasks.Task;

namespace TelegramBotExtensions.LongPolling
{
    public class LongPollingHostService : BackgroundService
    {
        private readonly IEnumerable<IBackGroundService> _backGroundServices;
        private readonly IServiceProvider _serviceProvider;
        private readonly UpdateQueue _updateQueue;
        private readonly ILogger<LongPollingHostService> _logger;
        public LongPollingHostService(IEnumerable<IBackGroundService> backGroundServices,IServiceProvider serviceProvider, UpdateQueue updateQueue, ILogger<LongPollingHostService> logger)
        {
            _backGroundServices = backGroundServices;
            _serviceProvider = serviceProvider;
            _updateQueue = updateQueue;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Thread1 : send getUpdateInfo request to telegram,and enqueue Update message to  UpdateMessageQueue
            foreach (var backGroundService in _backGroundServices)
                await backGroundService.Start(stoppingToken);
            
            _updateQueue.AfterEnqueueHandlerEvent += async () =>
            {
                var update = _updateQueue.Dequeue();
                var scope = _serviceProvider.CreateScope().ServiceProvider;
                var updateHandler = scope.GetService<IUpdateHandler>();
                await updateHandler.Process(update);
            };
            await CompletedTask;
        }

        public override Task StartAsync(CancellationToken token = default)
        {
            var clearWebhookInfoTask = ClearWebhookInfo(token);
            WaitAll(clearWebhookInfoTask);
            return base.StartAsync(token);
        }

        private async Task ClearWebhookInfo(CancellationToken token)
        {
            var telegramBot = _serviceProvider.CreateScope().ServiceProvider.GetService<ITelegramBotClient>();
            await telegramBot.TestApiAsync(token);
            await telegramBot.SetWebhookAsync("", cancellationToken: token);
        }

        
    }
}
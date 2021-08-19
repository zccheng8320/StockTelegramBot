using System;
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
        private readonly IServiceProvider _serviceProvider;
        private readonly IQueue<Update> _updateQueue;
        private readonly IConfiguration _configuration;
        private readonly ILogger<LongPollingHostService> _logger;
        private readonly int _longPollingMaximumNumberOfRequestProcessors;

        public LongPollingHostService(IServiceProvider serviceProvider, IQueue<Update> updateQueue,
            IConfiguration configuration, ILogger<LongPollingHostService> logger)
        {
            _serviceProvider = serviceProvider;
            _updateQueue = updateQueue;
            _configuration = configuration;
            _logger = logger;
            _longPollingMaximumNumberOfRequestProcessors =
                int.Parse(_configuration[$"TelegramSetting:LongPollingMaximumNumberOfRequestProcessors"]);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Run(() => UpdateInfoMonitor(stoppingToken), stoppingToken);
            Run(() => RequestProcessorController(stoppingToken), stoppingToken);
            return CompletedTask;

        }

        public override Task StartAsync(CancellationToken token = default)
        {
            var clearWebhookInfoTask = ClearWebhookInfo(token);
            WaitAll(clearWebhookInfoTask);
            return base.StartAsync(token);
        }

        /// <summary>
        /// UpdateInfoMonitor will send getUpdateInfo Request to get new <see cref="Update"/> object,which be enqueue to <see cref="UpdateQueue"/>
        /// </summary>
        private Task UpdateInfoMonitor(CancellationToken cancellationToken)
        {
            var currentOffset = 0;
            _logger.LogInformation("Start UpdateInfoMonitor");
            while (true)
            {
                try
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;
                    var task = SendGetUpdateRequestAsync(currentOffset, cancellationToken);
                    task.Wait(cancellationToken);
                    var updates = task.Result;
                    foreach (var update in updates)
                    {
                        _updateQueue.Enqueue(update);
                        currentOffset = update.Id;
                    }

                    if (updates.Any())
                        currentOffset++;
                }
                catch (Exception e)
                {
                    _logger.LogCritical(e.ToString());
                    _logger.LogInformation("ReStart UpdateInfoMonitor");
                }
            }

            return FromCanceled(cancellationToken);
        }
        private async Task ClearWebhookInfo(CancellationToken token)
        {
            var telegramBot = _serviceProvider.CreateScope().ServiceProvider.GetService<ITelegramBotClient>();
            await telegramBot.TestApiAsync(token);
            await telegramBot.SetWebhookAsync("", cancellationToken: token);
        }

        private async Task<Update[]> SendGetUpdateRequestAsync(int offset, CancellationToken token)
        {
            try
            {
                var telegramBotClient =
                    _serviceProvider.CreateAsyncScope().ServiceProvider.GetService<ITelegramBotClient>();
                //_logger.LogInformation($"Client is sending GetUpdates(offset:{offset}) request to Telegram Api...");
                var updates = await telegramBotClient.GetUpdatesAsync(offset,
                    _longPollingMaximumNumberOfRequestProcessors, cancellationToken: token);
                //_logger.LogInformation($"GetUpdates(offset:{offset}) request is success!, updates count is {updates.Length}");
                return updates;
            }
            catch (Exception e)
            {
                _logger.LogCritical(e.ToString());
                return new Update[0];
            }
        }

        private void RequestProcessorController(CancellationToken token)
        {
            for (var i = 0; i < _longPollingMaximumNumberOfRequestProcessors; i++)
                CreateUpdateHandlerProcessTask(token);
        }
        private void CreateUpdateHandlerProcessTask(CancellationToken cancellationToken)
        {
            var scope = _serviceProvider.CreateScope().ServiceProvider;
            var updateHandler = scope.GetService<IUpdateHandler>();
            var update = _updateQueue.Dequeue();
            var task = updateHandler.Process(update);
            task.ContinueWith(m =>
            {
                if(!cancellationToken.IsCancellationRequested)
                    CreateUpdateHandlerProcessTask(cancellationToken);
            }, cancellationToken);
        }
    }
}
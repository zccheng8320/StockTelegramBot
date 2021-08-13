using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBotExtensions.Interfaces;
using Timer = System.Threading.Timer;

namespace TelegramBotExtensions.LongPolling
{
    public class LongPollingExecutor : ILongPollingExecutor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IQueue<Update> _updateQueue;
        private readonly IConfiguration _configuration;
        private readonly ILogger<LongPollingExecutor> _logger;
        private readonly int _longPollingMaximumNumberOfRequestProcessors;
        public LongPollingExecutor(IServiceProvider serviceProvider, IQueue<Update> updateQueue, IConfiguration configuration, ILogger<LongPollingExecutor> logger)
        {
            _serviceProvider = serviceProvider;
            _updateQueue = updateQueue;
            _configuration = configuration;
            _logger = logger;
            _longPollingMaximumNumberOfRequestProcessors = int.Parse(_configuration[$"TelegramSetting:LongPollingMaximumNumberOfRequestProcessors"]);
        }
        public void StartUp()
        {
            Task.Run(UpdateInfoMonitor);
            Task.Run(RequestProcessorController);
            Console.ReadKey();
        }
        /// <summary>
        /// UpdateInfoMonitor will send getUpdateInfo Request to get new <see cref="Update"/> object,which be enqueue to <see cref="UpdateQueue"/>
        /// </summary>
        private void UpdateInfoMonitor()
        {
            var currentOffset = 0;
            _logger.LogInformation($"UpdateMonitor is started completed");
            while (true)
            {
                try
                {
                    var task = SendGetUpdateRequestAsync(currentOffset);
                    task.Wait();
                    var updates = task.Result;
                    foreach (var update in updates)
                    {
                        _updateQueue.Enqueue(update);
                        currentOffset = update.Id;
                    }
                    if (updates.Any())
                        currentOffset++;
                    task.Dispose();
                }
                catch (Exception e)
                {
                    _logger.LogCritical(e.ToString());
                }
            }
           
        }

        private async Task<Update[]> SendGetUpdateRequestAsync(int offset)
        {
            var telegramBotClient =
                _serviceProvider.CreateAsyncScope().ServiceProvider.GetService<ITelegramBotClient>();
            //_logger.LogInformation($"Client is sending GetUpdates(offset:{offset}) request to Telegram Api...");
            var updates = await telegramBotClient.GetUpdatesAsync(offset, _longPollingMaximumNumberOfRequestProcessors);
            //_logger.LogInformation($"GetUpdates(offset:{offset}) request is success!, updates count is {updates.Length}");
            return updates;
        }

        /// <summary>
        /// Star
        /// </summary>
        private void RequestProcessorController()
        {
            var maxTask = _longPollingMaximumNumberOfRequestProcessors;
            for (var i = 0; i < maxTask; i++)
                CreateUpdateHandlerProcessTask();
        }

        private void CreateUpdateHandlerProcessTask()
        {
            var scope = _serviceProvider.CreateScope().ServiceProvider;
            var updateHandler = scope.GetService<IUpdateHandler>();
            var update = _updateQueue.Dequeue();
            var task = updateHandler.Process(update);
            task.ContinueWith(m =>
            {
                CreateUpdateHandlerProcessTask();
                m.Dispose();
            });

        }

    }
}